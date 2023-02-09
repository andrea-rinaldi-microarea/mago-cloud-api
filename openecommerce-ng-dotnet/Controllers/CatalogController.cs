using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using openecommerce_ng_dotnet.Models;

namespace openecommerce_ng_dotnet.Controllers;
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