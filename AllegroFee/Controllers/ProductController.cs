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
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly string AllegroApiBaseUrl;
        private const string MeEndpoint = "me";

        public ProductController(IHttpClientFactory clientFactory, IAccessTokenProvider accessTokenProvider, IConfiguration configuration)
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

        [HttpGet("offer-fee-preview/{offerId}")]
        public async Task<IActionResult> GetOfferFeePreviewByOfferIdAsync(string offerId)
        {
            var offerDataResponse = await GetOfferDataAsync(offerId) as OkObjectResult;

            if (offerDataResponse == null)
            {
                return BadRequest("Failed to get offer data.");
            }

            var offerData = offerDataResponse.Value as JObject;

            var offerFeeResponse = await GetOfferFeePreviewAsync(offerData) as OkObjectResult;
            var response = offerFeeResponse.Value as JObject;
            return Ok(response);
        }
        
        [HttpPost("offer-fee-preview")]
        public async Task<IActionResult> GetOfferFeePreviewAsync([FromBody] JObject offerData)
        {
            // Retrieve the access token for the Allegro API
            var accessToken = await _accessTokenProvider.GetAccessForApplicationTokenAsync();

            // Create a new HttpClient for sending the request
            using var httpClient = new HttpClient();

            // Create a new HttpRequestMessage for the offer-fee-preview endpoint
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.allegro.pl.allegrosandbox.pl/pricing/offer-fee-preview");

            // Set the Accept header to specify the desired media type
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.allegro.public.v1+json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("Accept-Language", "PL");

            // Serialize the offer data to a JSON string
            string jsonString = JsonConvert.SerializeObject(new { offer = offerData });

            // Create the request content with the serialized JSON string and the appropriate content type
            var content = new StringContent(jsonString, Encoding.UTF8, "application/vnd.allegro.public.v1+json");

            // Set the request content
            request.Content = content;

            // Send the request and get the response
            var response = await httpClient.SendAsync(request);

            // Ensure the response was successful (status code 2xx)
            response.EnsureSuccessStatusCode();

            // Read the response content as a string
            string responseContent = await response.Content.ReadAsStringAsync();
            return Ok(JObject.Parse(responseContent));
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
        
        
        [HttpGet("offer-data/{offerId}")]
        public async Task<IActionResult> GetOfferDataAsync(string offerId)
        {
            try
            {
                var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
                var request = CreateAllegroApiRequest($"sale/product-offers/{offerId}", accessToken);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await SendAllegroApiRequest(request);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return NotFound();
                    }
                    return BadRequest($"Failed to get offer data. StatusCode={response.StatusCode} Reason={response.ReasonPhrase}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JObject.Parse(responseString);
                //var wrappedResponseObject = new JObject(new JProperty("offer", responseObject));
                return Ok(responseObject);
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
            var request = CreateAllegroApiRequest(apiUrl, accessToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await SendAllegroApiRequest(request);
        }
        #endregion
        
        #region Private Methods
        private async Task<IEnumerable<PartialProductResponse>> HandleUserProductsResponse(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var userProducts = DeserializeJson<IEnumerable<PartialProductResponse>>(responseString);

            return userProducts;
        }
        private async Task<JObject> GetCurrentUserDetailsAsync()
        {
            var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
            var request = CreateAllegroApiRequest("me", accessToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await SendAllegroApiRequest(request);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var userDetails = JObject.Parse(responseString);

            return userDetails;
        }
        private Category CreateCategoryFromResponse(CategoryResponse response)
        {
            return new Category
            {
                Id = response.Id,
                Name = response.Name,
            };
        }
        private async Task<HttpResponseMessage> GetProductResponseAsync(string productId)
        {
            var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
            var request = CreateAllegroApiRequest($"sale/offers/{productId}", accessToken);
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