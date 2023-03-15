using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AllegroFee.Models;
using AllegroFee.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Attribute = AllegroFee.Models.Attribute;

namespace YourProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IAccessTokenProvider _accessTokenProvider;
        private string YOUR_ACCESS_TOKEN = "";
        private readonly string AllegroApiBaseUrl;
        private const string MeEndpoint = "me";

        public ProductController(IHttpClientFactory clientFactory, IAccessTokenProvider accessTokenProvider, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _accessTokenProvider = accessTokenProvider;
            AllegroApiBaseUrl = configuration.GetValue<string>("AllegroApiBaseUrl");
        }

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
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpGet("categories/{categoryId}")]
        public async Task<IActionResult> GetCategory(string categoryId)
        {
            try
            {
                var accessToken = await _accessTokenProvider.GetAccessForApplicationTokenAsync();
                var request = CreateAllegroApiRequest($"sale/categories/{categoryId}");
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
        
        private Category CreateCategoryFromResponse(CategoryResponse response)
        {
            return new Category
            {
                Id = response.Id,
                Name = response.Name,
            };
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProduct(string productId)
        {
            try
            {
                var response = await GetProductResponseAsync(productId);

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var productResponse = await HandleProductResponse(response);

                return Ok(productResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user-products")]
        public async Task<IActionResult> GetUserProducts()
        {
            try
            {
                var userDetails = await GetCurrentUserDetailsAsync();
                string userId = userDetails["id"].ToString();

                string apiUrl = $"https://api.allegro.pl.allegrosandbox.pl/sale/user-offers?user.id={userId}";

                var response = await GetUserProductsResponseAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var userProductsResponse = await HandleUserProductsResponse(response);

                return Ok(userProductsResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<HttpResponseMessage> GetUserProductsResponseAsync(string apiUrl)
        {
            var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
            var request = CreateAllegroApiRequest(apiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await SendAllegroApiRequest(request);
        }

        private async Task<IEnumerable<PartialProductResponse>> HandleUserProductsResponse(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var userProducts = DeserializeJson<IEnumerable<PartialProductResponse>>(responseString);

            return userProducts;
        }

        private async Task<JObject> GetCurrentUserDetailsAsync()
        {
            var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
            var request = CreateAllegroApiRequest("me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await SendAllegroApiRequest(request);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var userDetails = JObject.Parse(responseString);

            return userDetails;
        }

        #region Methods
        private async Task<HttpResponseMessage> GetProductResponseAsync(string productId)
        {
            var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
            var request = CreateAllegroApiRequest($"sale/offers/{productId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await SendAllegroApiRequest(request);
            return response;
        }

        private async Task<PartialProductResponse> HandleProductResponse(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var productResponse = DeserializeJson<PartialProductResponse>(responseString);
            return productResponse;
        }


        private HttpRequestMessage CreateAllegroApiRequest(string relativeUrl)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{AllegroApiBaseUrl}/{relativeUrl}");
            request.Headers.Add("Authorization", $"Bearer {YOUR_ACCESS_TOKEN}");
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

        private Product CreateProductFromResponse(ProductResponse productResponse)
        {
            return new Product
            {
                Id = productResponse.Id,
                Name = productResponse.Name,
                Description = productResponse.Description,
                Category = productResponse.Category,
                SellingMode = productResponse.SellingMode,
                Images = productResponse.Images?.Pictures?.Data,
                Attributes = productResponse.Parameters?.Parameters?.Data?.Select(p => new Attribute
                {
                    Id = p.Id,
                    Name = p.Name,
                    Values = p.Values?.Select(v => new AttributeValue { Value = v.Value }).ToList()
                }).ToList(),
                Vendor = productResponse.Seller,
                Stock = productResponse.Stock,
                Condition = productResponse.Condition,
                Ean = productResponse.Ean
            };
        }

        #endregion

        

    }
public class PartialProductResponse
{
    [JsonProperty("id")]
    public string ProductId { get; set; }
    public string Name { get; set; }
    public Category Category { get; set; }
    [JsonProperty("images")]
    public List<Image> Images { get; set; }
    [JsonProperty("sellingMode")]
    public SellingMode SellingMode { get; set; }
    [JsonProperty("tax")]
    public Tax Tax { get; set; }
    [JsonProperty("stock")]
    public Stock Stock { get; set; }
    [JsonProperty("promotion")]
    public Promotion Promotion { get; set; }
}
}