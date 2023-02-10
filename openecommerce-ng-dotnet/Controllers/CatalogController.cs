using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using openecommerce_ng_dotnet.Models;

namespace openecommerce_ng_dotnet.Controllers;

class MagoAPIHeaders
{
    private IMagoConnection magoConnection;
    public MagoAPIHeaders(IMagoConnection mc)
    {
        magoConnection = mc;
    }

    public static class Names
    {
        public static string Authorization = "Authorization";
        public static string Producer = "Producer-Info";
    }

    public string authorizationData { get {
        return JsonConvert.SerializeObject(new
        {
            Type = "jwt",
            appId = "MagoAPI", 
            SecurityValue = magoConnection.TbUserData.Token
        });
    }}

    public string producerInfo { get {
        return JsonConvert.SerializeObject(new
        {
            ProducerKey = magoConnection.APIClient.ProducerInfo.ProducerKey,
            AppKey = magoConnection.APIClient.ProducerInfo.AppKey
        });
    }}
}

[ApiController]
[Route("[controller]")]
public class CatalogController : ControllerBase
{
    private IMagoConnection magoConnection;
    private readonly IHttpClientFactory httpClientFactory;

    public CatalogController
            (
                IMagoConnection m,
                IHttpClientFactory h
            )
    {
        magoConnection = m;
        httpClientFactory = h;
    }

#region helper classes
    private class Filter
    {
        public string key { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }

    private class ResponseData
    {
        public int totalRecordsNumber { get; set; } = 0;
        public int currentPageRecordsNumber { get; set; } = 0;
        public int currentPageNumber { get; set; } = 0;

        public object[] records { get; set; } = new object[] {};
    }

    private class Data
    {
        public string       response { get; set; } = string.Empty;
        public ResponseData responseData { get; set; } = new ResponseData();
    }

    private class ShopAssortments
    {
        public string Assortment { get; set; } = string.Empty;
    }

    private class AssortmentsItems
    {
        public string Item { get; set; } = string.Empty;
        public string RetailCtg { get; set; } = string.Empty;

    }

    private class RetailCtgItem
    {
        public string Item { get; set; } = string.Empty;
    }

    private class MagoItem
    {
        public string Item { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Baseprice { get; set; } = 0.0;
    }

    private class CatalogItem
    {
        public string Item { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Price { get; set; } = 0.0;
    }
#endregion

    private async Task<List<T>> GetDataTable<T>
        (
            string      tableName,
            string[]    fields,
            Filter[]    filters,
            int         recordsPerPage
        )
    {
        var mh = new MagoAPIHeaders(magoConnection);
        
        var baseMSHUrl = magoConnection?.APIClient?.MagoServicesHub.GetServiceUrl(magoConnection?.TbUserData?.SubscriptionKey);
        var msg = new HttpRequestMessage
        (
            HttpMethod.Post,
            $"{baseMSHUrl}/magoservices/getDataTable"
        );

        msg.Headers.TryAddWithoutValidation(HeaderNames.ContentType,            "application/json");
        msg.Headers.TryAddWithoutValidation(MagoAPIHeaders.Names.Authorization, mh.authorizationData);
        msg.Headers.TryAddWithoutValidation(MagoAPIHeaders.Names.Producer,      mh.producerInfo);

        var query = JsonConvert.SerializeObject(new
        {
            tableName = tableName,
            fieldsName = fields,
            filters = filters,
            recordsPerPage = recordsPerPage
        });
        msg.Content = new StringContent
            (
                content: query, 
                encoding: System.Text.Encoding.UTF8, 
                mediaType: "application/json"
            );
        
        var httpClient = httpClientFactory.CreateClient();

        using var response = await httpClient.SendAsync(msg);
        response.EnsureSuccessStatusCode();
        
        string result = response.Content.ReadAsStringAsync().Result;

        if (string.IsNullOrEmpty(result))
        {
            throw new Exception("Invalid response from getDataTable.");
        }

        Data? data = JsonConvert.DeserializeObject<Data>(result);

        if (data == null || data?.response != "OK")
        {
            throw new Exception("Call to getDataTable not successful.");
        }

        var ret = new List<T>();

        foreach (JObject e in data.responseData.records)
        {
            ret.Add(e.ToObject<T>());
        }
        return ret;
    }

    [HttpPost("synchronizeCatalog")]
    public async Task<ActionResult<bool>> SynchronizeCatalog([FromBody] CatalogRequest catalogRequest)
    {
        if (magoConnection.APIClient == null)
        {
            return new ContentResult {
                StatusCode = 500,
                Content = "Error on synchronizeCatalog: not connected to back-end"
            };
        }

        var catalog = new List<CatalogItem>();
        try
        {
            var assortments = await GetDataTable<ShopAssortments>
                (
                    "MA_RMAssortmentsShops",
                    new string[] { "Assortment" },
                    new Filter[] {
                        new Filter {
                            key = "Shop",
                            value = catalogRequest.filter
                        }
                    },
                    catalogRequest.recordsPerPage
                );

            foreach (var asmt in assortments)
            {
                var asmtItems = await GetDataTable<AssortmentsItems>
                    (
                        "MA_RMAssortmentsItems",
                        new string[] { "Item", "RetailCtg" },
                        new Filter[] {
                            new Filter {
                                key = "Assortment",
                                value = asmt.Assortment
                            }
                        },
                        catalogRequest.recordsPerPage
                    );

                foreach (var itmctg in asmtItems)
                {
                    if (itmctg.Item != string.Empty)
                    {
                        var itm = await GetDataTable<MagoItem>
                            (
                                "MA_Items",
                                new string[] { "item", "description", "baseprice" },
                                new Filter[] {
                                    new Filter {
                                        key = "item",
                                        value = itmctg.Item
                                    }
                                },
                                catalogRequest.recordsPerPage
                            );
                        foreach (var i in itm)
                        {
                            catalog.Add(new CatalogItem() {
                                Item = i.Item,
                                Description = i.Description,
                                Price = i.Baseprice
                            });
                        }
                    }
                    else
                    {
                        var itms = await GetDataTable<RetailCtgItem>
                            (
                                "MA_RMItemsCategories",
                                new string[] { "item" },
                                new Filter[] {
                                    new Filter {
                                        key = "RetailCtg",
                                        value = itmctg.RetailCtg
                                    }
                                },
                                catalogRequest.recordsPerPage
                            );
                        foreach (var i in itms)
                        {
                            var itm = await GetDataTable<MagoItem>
                                (
                                    "MA_Items",
                                    new string[] { "item", "description", "baseprice" },
                                    new Filter[] {
                                        new Filter {
                                            key = "item",
                                            value = i.Item
                                        }
                                    },
                                    catalogRequest.recordsPerPage
                                );
                            foreach (var j in itm)
                            {
                                catalog.Add(new CatalogItem() {
                                    Item = j.Item,
                                    Description = j.Description,
                                    Price = j.Baseprice,
                                    Category = itmctg.RetailCtg
                                });
                            }
                        }
                    }
                }
            }

            return true;
        }
        catch (System.Exception e)
        {
            return new ContentResult {
                StatusCode = 500,
                Content = e.Message
            };        
        }
   }

    [HttpPost("getCatalog")]
    public async Task<ActionResult<string>> GetCatalog([FromBody] CatalogRequest catalogRequest)
    {
        if (magoConnection.APIClient == null)
        {
            return new ContentResult {
                StatusCode = 500,
                Content = "Error on getCatalog: not connected to back-end"
            };
        }
        try
        {
            var authorizationData = JsonConvert.SerializeObject(new
            {
                Type = "jwt",
                appId = "MagoAPI", 
                SecurityValue = magoConnection.TbUserData.Token
            });

            var producerInfo = JsonConvert.SerializeObject(new
            {
                ProducerKey = magoConnection.APIClient.ProducerInfo.ProducerKey,
                AppKey = magoConnection.APIClient.ProducerInfo.AppKey
            });

            var baseMSHUrl = magoConnection.APIClient.MagoServicesHub.GetServiceUrl(magoConnection.TbUserData.SubscriptionKey);
            var msg = new HttpRequestMessage
                (
                    HttpMethod.Post,
                    $"{baseMSHUrl}/magoservices/getDataTable"
                );
            msg.Headers.TryAddWithoutValidation(HeaderNames.ContentType,     "application/json");
            msg.Headers.TryAddWithoutValidation("Authorization",             authorizationData);
            msg.Headers.TryAddWithoutValidation("Producer-Info",             producerInfo);

            var query = JsonConvert.SerializeObject(new
            {
                tableName = "MA_Items",
                fieldsName = new string[] { "item", "description", "baseprice" },
                filters = new object[] {
                    new {
                        key = "commodityctg",
                        value = catalogRequest.filter
                    }
                },
                recordsPerPage = catalogRequest.recordsPerPage
            });

            msg.Content = new StringContent
                (
                    content: query, 
                    encoding: System.Text.Encoding.UTF8, 
                    mediaType: "application/json"
                );
            
            var httpClient = httpClientFactory.CreateClient();

            using var response = await httpClient.SendAsync(msg);
            response.EnsureSuccessStatusCode();
            
            string result = response.Content.ReadAsStringAsync().Result;

            if (string.IsNullOrEmpty(result))
            {
                return new ContentResult {
                    StatusCode = 500,
                    Content = "Invalid response"
                };
            }

            return result;
        }
        catch (System.Exception e)
        {
            return new ContentResult {
                StatusCode = 500,
                Content = e.Message
            };
        }
    }
}