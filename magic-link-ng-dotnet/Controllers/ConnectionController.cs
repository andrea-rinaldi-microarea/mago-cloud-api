using System.Net.Http;
using System.Threading.Tasks;
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
        private HttpClient httpClient = new HttpClient();

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginRequest, string url)
        {
            try
            {
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
        public async Task<ActionResult<bool>> GetData([FromBody] GetDataRequest getDataRequest, string url)
        {
            try
            {
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
    }
}