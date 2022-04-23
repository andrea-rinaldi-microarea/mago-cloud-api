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

using Microarea.Tbf.Model.API;
using Microarea.Tbf.Model.Interfaces.API;

namespace magic_link_ng_dotnet.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConnectionController : ControllerBase
    {
        private readonly MagoAPIClientWrapper _magoAPI;

        public ConnectionController(MagoAPIClientWrapper magoAPI)
        {
            _magoAPI = magoAPI;
        }

        [HttpPost("login")]
        public async Task<ActionResult<TbUserData>> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                _magoAPI.client = new MagoAPIClient(loginRequest.url, new ProducerInfo("MyProdKey", "MyAppId"));

                IAccountManagerResult result = await _magoAPI.client.AccountManager?.Login(loginRequest.accountName, loginRequest.password, loginRequest.subscriptionKey);

                if (!result.Success || result.UserData == null || !result.UserData.IsLogged)
                {
                    return new ContentResult {
                        StatusCode = result.StatusCode,
                        Content = $"Error on login: {result.ReturnValue}"
                    };
                }

                return (TbUserData)result.UserData;
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
        public async Task<ActionResult<bool>> Logout([FromBody] TbUserDataWrapper userData)
        {
            if (_magoAPI.client == null)
            {
                return new ContentResult {
                    StatusCode = 500,
                    Content = "Error on logout: not logged in"
                };
            }
            try
            {
                IAccountManagerResult isValid = await _magoAPI.client.AccountManager.IsValid(userData.Token, userData.SubscriptionKey);
                if (isValid.Success)
                {
                    IAccountManagerResult result = await _magoAPI.client.AccountManager.Logout(userData.Token, userData.SubscriptionKey);
                    if (result.Success)
                    {
                        _magoAPI.client = null;
                        return true;
                    }
                    else
                    {
                        return new ContentResult {
                            StatusCode = result.StatusCode,
                            Content = $"Error on logout: {result.ReturnValue}"
                        };
                    }
                }
                else
                {
                    return new ContentResult {
                        StatusCode = 500,
                        Content = "Login no more valid, logout failed."
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

        [HttpPost("getData")]
        public async Task<ActionResult<IEnumerable<string>>> GetData([FromBody] GetDataRequest request)
        {
            if (_magoAPI.client == null)
            {
                return new ContentResult {
                    StatusCode = 500,
                    Content = "Error on logout: not logged in"
                };
            }
            try
            {
                string xmlParams = request.xmlParams;
                ITbServerMagicLinkResult tbResult = await _magoAPI.client.TbServer?.GetXmlData(request.userData, request.xmlParams, DateTime.Now);

                if (!tbResult.Success)
                {
                    return new ContentResult {
                        StatusCode = 500,
                        Content = $"Error in GetData\r\n{tbResult.ReturnValue}"
                    };
                }
                if (tbResult.Xmls.Count == 0)
                {
                    return new ContentResult {
                        StatusCode = 500,
                        Content = "No data extracted"
                    };
                }

                // just 1 result can be an error or warning message
                if (tbResult.Xmls.Count == 1)
                {
                    var strMessages = extractXMLMessages(tbResult.Xmls[0], out bool hasErrors);
                    if (hasErrors)
                    {
                        return new ContentResult {
                            StatusCode = 500,
                            Content = $"Request failed:\r\n{strMessages}"
                        };
                    }
                    else if (strMessages != string.Empty)
                    {
                        return new ContentResult {
                            StatusCode = 500,
                            Content = $"Request completed with warnings:\r\n{strMessages}"
                        };
                    }
                }

                return tbResult.Xmls;
            }
            catch (System.Exception e)
            {
                return new ContentResult {
                    StatusCode = 500,
                    Content = e.Message
                };
            }
        }

        [HttpPost("setData")]
        public async Task<ActionResult<SetDataResponse>> SetData([FromBody] SetDataRequest request)
        {
            if (_magoAPI.client == null)
            {
                return new ContentResult {
                    StatusCode = 500,
                    Content = "Error on logout: not logged in"
                };
            }
            try
            {
                ITbServerMagicLinkResult tbResult = await _magoAPI.client.TbServer?.SetXmlData(request.userData, request.xmlData, 0, DateTime.Now);

                if (!tbResult.Success)
                {
                    return new ContentResult {
                        StatusCode = 500,
                        Content = $"Error in SetData\r\n{tbResult.ReturnValue}"
                    };
                }
                if (tbResult.Xmls.Count == 0)
                {
                    return new ContentResult {
                        StatusCode = 500,
                        Content = "No postback received"
                    };
                }

                var strMessages = extractXMLMessages(tbResult.Xmls[0], out bool hasErrors);
                if (hasErrors)
                {
                    return new ContentResult {
                        StatusCode = 500,
                        Content = $"Request failed:\r\n{strMessages}"
                    };
                }

                return new SetDataResponse {
                    xmlData = tbResult.Xmls[0],
                    warnings = strMessages
                }; 
            }
            catch (System.Exception e)
            {
                return new ContentResult {
                    StatusCode = 500,
                    Content = e.Message
                };
            }
        }

        private string extractXMLMessages(string xmlResult, out bool hasErrors)
        {
            hasErrors = false;
            if (xmlResult == string.Empty)
                return string.Empty;

            XDocument xmlMessages = XDocument.Parse(xmlResult);
            XmlNamespaceManager mgr = new XmlNamespaceManager(new NameTable());
            var ns = xmlMessages.Root.Name.Namespace;
            mgr.AddNamespace("maxs", ns.NamespaceName);

            string resultText = string.Empty;

            IEnumerable<XElement> elems = xmlMessages.XPathSelectElements("//maxs:Warning", mgr);
            if (elems.Count() > 0)
            {
                resultText += "\r\nWarnings:\r\n";
                foreach (XElement elem in elems)
                {
                    resultText += elem.XPathSelectElement("maxs:Message", mgr).Value;
                }
            }
            elems = xmlMessages.XPathSelectElements("//maxs:Error", mgr);
            if (elems.Count() > 0)
            {
                hasErrors = true;
                resultText += "\r\nErrors:\r\n";
                foreach (XElement elem in elems)
                {
                    resultText += elem.XPathSelectElement("maxs:Message", mgr).Value;
                }
            }
            return resultText;
        }
    }
}
