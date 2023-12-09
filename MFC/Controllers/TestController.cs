using System.Net;
using System.Net.Http.Headers;
using System.Text;
using MFC.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MFC.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly string AllegroApiBaseUrl;
        private const string MeEndpoint = "me";

        public TestController(IHttpClientFactory clientFactory, IAccessTokenProvider accessTokenProvider, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _accessTokenProvider = accessTokenProvider;
            AllegroApiBaseUrl = configuration.GetValue<string>("AllegroApiBaseUrl");
        }
        
        #region Action Methods
        [HttpGet("check-token")]
        public async Task<IActionResult> CheckTokenForApplication()
        {
            var accessToken = await _accessTokenProvider.GetAccessForApplicationTokenAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{AllegroApiBaseUrl}/{MeEndpoint}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await SendAllegroApiRequest(request);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                // do something with the response
                return Ok(responseString);
            }
            return BadRequest();
        }
        private async Task<HttpResponseMessage> SendAllegroApiRequest(HttpRequestMessage request)
        {
            var client = _clientFactory.CreateClient();
            return await client.SendAsync(request);
        }
        #endregion
    }
}