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

        
        [HttpGet("{productId}/sales")]
        public async Task<IActionResult> GetSales(string productId)
        {
            var requestUrl = $"{AllegroApiBaseUrl}/sale/demand?offerId={productId}&period=365";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {YOUR_ACCESS_TOKEN}");

            var response = await SendAllegroApiRequest(request);

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var sales = DeserializeSalesResponse(responseString);

            return Ok(sales);
        }
        
        [HttpGet("transactions/{productId}")]
        public async Task<IActionResult> GetTransactions(string productId)
        {
            var requestUrl = $"{AllegroApiBaseUrl}/sale/demand?offerId={productId}&period=365";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {YOUR_ACCESS_TOKEN}");

            var response = await SendAllegroApiRequest(request);

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var sales = DeserializeSalesResponse(responseString);

            return Ok(sales);
        }
        
        private List<Sale>? DeserializeSalesResponse(string responseString)
        {
            var salesResponse = JsonConvert.DeserializeObject<SalesResponse>(responseString);
            if (salesResponse == null || salesResponse.Sales == null)
            {
                return null;
            }
            return salesResponse.Sales.Select(s => new Sale
            {
                Date = s.Date,
                Quantity = s.Quantity
            }).ToList();
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
        private decimal GetDiscountForPromotions(List<Promotion> promotions, decimal totalValue)
        {
            decimal discount = 0m;
            foreach (var promotion in promotions)
            {
                switch (promotion.Type)
                {
                    case PromotionType.PercentageDiscount:
                        discount += totalValue * (promotion.Amount / 100m);
                        break;
                    case PromotionType.FixedDiscount:
                        discount += Math.Min(promotion.Amount, totalValue);
                        break;
                    case PromotionType.FreeDelivery:
                        discount += promotion.Amount;
                        break;
                    default:
                        break;
                }
            }
            return discount;
        }

        #endregion

        

    }
    public class PartialProductResponse
    {
        [JsonProperty("id")]
        public string ProductId { get; set; }
        public string Name { get; set; }
        public Category Category { get; set; }
    }
}