using System.Net;
using System.Net.Http.Headers;
using System.Text;
using AllegroFee.Responses;
using AllegroFee.Services;
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
        private readonly CategoryService _categoryService;


        public ProductController(IHttpClientFactory clientFactory, IAccessTokenProvider accessTokenProvider, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _accessTokenProvider = accessTokenProvider;
            AllegroApiBaseUrl = configuration.GetValue<string>("AllegroApiBaseUrl");
        }
        
        #region Action Methods

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
            // Convert JObject to string
            var response = offerFeeResponse.Value.ToString(); 
            // Return string content as JSON
            return Content(response, "application/json"); 
        }
        
        [HttpPost("offer-fee-preview")]
        public async Task<IActionResult> GetOfferFeePreviewAsync([FromBody] JObject offerData)
        {
            // Retrieve the access token for the Allegro API
            var accessToken = await _accessTokenProvider.GetAccessForApplicationTokenAsync();
            
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
            request.Content = content;

            // Send the request and get the response
            var response = await httpClient.SendAsync(request);

            // Ensure the response was successful (status code 2xx)
            response.EnsureSuccessStatusCode();

            // Read the response content as a string
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(responseContent);
            return Ok(result);
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
}