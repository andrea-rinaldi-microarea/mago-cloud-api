using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using magic_link_ng_dotnet.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace magic_link_ng_dotnet.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConnectionController : ControllerBase
    {

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginRequest, string url)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var response = await httpClient.PostAsync(
                    url, 
                    new StringContent(JsonConvert.SerializeObject(loginRequest), System.Text.Encoding.UTF8, "application/json")
                );
                if (!response.IsSuccessStatusCode)
                {
                    return new ContentResult {
                        StatusCode = (int)response.StatusCode,
                        Content = $"Error on login: {response.ReasonPhrase}"
                    };
                }

                string result = response.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(result))
                {
                    return new ContentResult {
                        StatusCode = 500,
                        Content = "Invalid login"
                    };
                }

                var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(result);
                return loginResponse;
            }
            catch (System.Exception e)
            {
                return new ContentResult {
                    StatusCode = 500,
                    Content = e.Message
                };
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult<bool>> Logout([FromBody] AuthorizationData authorizationData, string url)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, url);
                msg.Headers.TryAddWithoutValidation("Authorization", JsonConvert.SerializeObject(authorizationData));
                msg.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                var response = await httpClient.SendAsync(msg);

                if (!response.IsSuccessStatusCode)
                {
                    return new ContentResult {
                        StatusCode = (int)response.StatusCode,
                        Content = $"Error on logout: {response.ReasonPhrase}"
                    };
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

        [HttpPost("getData")]
        public async Task<ActionResult<IEnumerable<string>>> GetData([FromBody] GetDataRequest getDataRequest, string url)
        {
            try
            {
                // Step 1 - invoke the initTBLogin to establish the communication with the back-end
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                HttpClient httpClient = new HttpClient(httpClientHandler);
                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, url + "tbserver/api/tb/document/initTBLogin/");
                msg.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                msg.Headers.TryAddWithoutValidation("Authorization", JsonConvert.SerializeObject(getDataRequest.authorizationData));
                msg.Headers.TryAddWithoutValidation("Server-Info", JsonConvert.SerializeObject(getDataRequest.serverInfo));
                var response = await httpClient.SendAsync(msg);

                if (!response.IsSuccessStatusCode)
                {
                    return new ContentResult {
                        StatusCode = (int)response.StatusCode,
                        Content = $"Error on GetData - initTBLogin: {response.ReasonPhrase}"
                    };
                }

                string result = response.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(result))
                {
                    return new ContentResult {
                        StatusCode = 500,
                        Content = "GetData - initTBLogin: Invalid response"
                    };
                }
                JObject jResult = JsonConvert.DeserializeObject<JObject>(result);
                JToken ok = jResult["success"];
                bool bOk = ok == null ? false : ok.Value<bool>();
                //@@TODO manage the case for "success" = true and "hasError" = true

                // Step 2 - extract the cookies for the subsequent call
                IEnumerable<Cookie> cookies = null;
                if (bOk)
                {
                    cookies = httpClientHandler.CookieContainer.GetCookies(msg.RequestUri);
                }
                else
                {
                    return new ContentResult {
                        StatusCode = 500,
                        Content = "GetData - initTBLogin: Request failed - " + jResult["message"]["text"].Value<string>()
                    };
                }

                // Step 3 - invoke the GetData via REST
                var functionParams = JsonConvert.SerializeObject(new
                {
                    ns = "Extensions.XEngine.TBXmlTransfer.GetDataRest",
                    args = new
                    {
                        param = getDataRequest.payload,
                        useApproximation = true,
                        loginName = getDataRequest.loginName,
                        result = "data"
                    }
                });

                msg = new HttpRequestMessage(HttpMethod.Post, url + "tbserver/api/tb/document/runRestFunction/");
                msg.Content = new StringContent(content: functionParams, encoding: Encoding.UTF8, mediaType: "application/json");
                msg.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                msg.Headers.TryAddWithoutValidation("Authorization", JsonConvert.SerializeObject(getDataRequest.authorizationData));

                httpClientHandler = new HttpClientHandler();
                httpClientHandler.CookieContainer = new CookieContainer();
                foreach (var cookie in cookies)
                {
                    httpClientHandler.CookieContainer.Add(msg.RequestUri, cookie);
                }
                httpClient = new HttpClient(httpClientHandler);

                response = await httpClient.SendAsync(msg);

                if (!response.IsSuccessStatusCode)
                {
                    return new ContentResult {
                        StatusCode = (int)response.StatusCode,
                        Content = $"Error on GetData - runRestFunction: {response.ReasonPhrase}"
                    };
                }

                result = response.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(result))
                {
                    return new ContentResult {
                        StatusCode = 500,
                        Content = "GetData - runRestFunction: Invalid response"
                    };
                }
                jResult = JsonConvert.DeserializeObject<JObject>(result);
                bool bSuccess = (jResult["success"] != null) ? jResult["success"].Value<bool>() : false;
                bool bRetVal = (jResult["retVal"] != null) ? jResult["retVal"].Value<bool>() : false;

                if (bSuccess && bRetVal) 
                {
                    IEnumerable<string> data = jResult["result"].Values<string>();
                    return new List<string>(data);
                }
                else if (bSuccess && !bRetVal)
                {
                    string errors, warnings;
                    extractXMLMessages((JToken)jResult["result"], out errors, out warnings);
                    if (errors != string.Empty)
                    {
                        return new ContentResult {
                            StatusCode = 500,
                            Content = "GetData - runRestFunction: Request failed - " + errors + warnings
                        };
                    }
                    else 
                    {
                        return new ContentResult {
                            StatusCode = 501, // not implemented -> warning
                            Content = "GetData - runRestFunction: " + warnings
                        };
                    }
                }
                else
                {
                    return new ContentResult {
                        StatusCode = 500,
                        Content = "GetData - runRestFunction: Request failed - " + jResult["message"].Value<string>()
                    };
                }
            }
            catch (System.Exception e)
            {
                return new ContentResult {
                    StatusCode = 500,
                    Content = e.Message
                };
            }
        }

        private void extractXMLMessages(JToken result, out string errors, out string warnings)
        {
            errors = string.Empty;
            warnings = string.Empty;
            if (result == null)
                return;

            XDocument xmlMessages = XDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(result.ToArray()[0].ToString())));
            var ns = xmlMessages.Root.Name.Namespace;
            XmlNamespaceManager mgr = new XmlNamespaceManager(new NameTable());
            mgr.AddNamespace("maxs", ns.NamespaceName);

            IEnumerable<XElement> elems = xmlMessages.XPathSelectElements("//maxs:Error", mgr);
            if (elems.Count() > 0)
            {
                foreach (XElement elem in elems)
                {
                    errors += " " + elem.XPathSelectElement("maxs:Message", mgr).Value;
                }
            }
            elems = xmlMessages.XPathSelectElements("//maxs:Warning", mgr);
            if (elems.Count() > 0)
            {
                foreach (XElement elem in elems)
                {
                    warnings += " " + elem.XPathSelectElement("maxs:Message", mgr).Value;
                }
            }
        }

    }
}