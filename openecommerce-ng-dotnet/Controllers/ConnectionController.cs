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
using openecommerce_ng_dotnet.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Microarea.Tbf.Model.API;
using Microarea.Tbf.Model.Interfaces.API;

namespace openecommerce_ng_dotnet.Controllers;
[ApiController]
[Route("[controller]")]
public class ConnectionController : ControllerBase
{
    private IMagoAPIClientWrapper _magoAPI;

    public ConnectionController(IMagoAPIClientWrapper magoAPI)
    {
        _magoAPI = magoAPI;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TbUserData>> Login([FromBody] LoginRequest loginRequest)
    {
        try
        {
            _magoAPI.Client = new MagoAPIClient(loginRequest.url, new ProducerInfo("MyProdKey", "MyAppId"));

            IGwamResult result = await _magoAPI?.Client?.GwamClient?.Login(loginRequest.accountName, loginRequest.password, loginRequest.subscriptionKey);

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
        if (_magoAPI.Client == null)
        {
            return new ContentResult {
                StatusCode = 500,
                Content = "Error on logout: not logged in"
            };
        }
        try
        {
            IAccountManagerResult isValid = await _magoAPI.Client.AccountManager.IsValid(userData.Token, userData.SubscriptionKey);
            if (isValid.Success)
            {
                IAccountManagerResult result = await _magoAPI.Client.AccountManager.Logout(userData.Token, userData.SubscriptionKey);
                if (result.Success)
                {
                    _magoAPI.Client = null;
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
}