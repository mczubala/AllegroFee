using System.Net;
using System.Net.Http.Headers;
using System.Text;
using AllegroFee.Models;
using AllegroFee.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AllegroFee.Controllers
{
    [ApiController]
    [Route("categories")]
    public class CategoryController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly string AllegroApiBaseUrl;

        public CategoryController(IHttpClientFactory clientFactory, IAccessTokenProvider accessTokenProvider, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _accessTokenProvider = accessTokenProvider;
            AllegroApiBaseUrl = configuration.GetValue<string>("AllegroApiBaseUrl");
        }
        
        #region Action Methods

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetCategory(string categoryId)
        {
            try
            {
                var accessToken = await _accessTokenProvider.GetAccessForApplicationTokenAsync();
                var request = CreateAllegroApiRequest($"sale/categories/{categoryId}", accessToken);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await SendAllegroApiRequest(request);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return NotFound();
                    }
                    return BadRequest($"Failed to get category. StatusCode={response.StatusCode} Reason={response.ReasonPhrase}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var categoryResponse = DeserializeJson<CategoryResponse>(responseString);
                var category = CreateCategoryFromResponse(categoryResponse);

                return Ok(category);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("categories/{categoryId}")]
        public async Task<IActionResult> GetSellingConditionsForCategoryAsync(string categoryId)
        {
            var accessToken = await _accessTokenProvider.GetAccessForApplicationTokenAsync();
            using var httpClient = new HttpClient();
            string relativeUrl = $"sale/categories/{categoryId}/selling-conditions";
            var request = CreateAllegroApiRequest(relativeUrl, accessToken);
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            return Ok(JObject.Parse(responseContent));
        }
        #endregion
        
        #region Private Methods
        private Category CreateCategoryFromResponse(CategoryResponse response)
        {
            return new Category
            {
                Id = response.Id,
                Name = response.Name, };
        }

        private HttpRequestMessage CreateAllegroApiRequest(string relativeUrl, string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{AllegroApiBaseUrl}/{relativeUrl}");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            return request;
        }

        private async Task<HttpResponseMessage> SendAllegroApiRequest(HttpRequestMessage request)
        {
            var client = _clientFactory.CreateClient();
            return await client.SendAsync(request);
        }

        private T DeserializeJson<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
        
        #endregion
    }
}