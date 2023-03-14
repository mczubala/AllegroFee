using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace YourProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly string YOUR_ACCESS_TOKEN = "";

        public ProductController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private const string AllegroApiBaseUrl = "https://api.allegro.pl";
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProduct(string productId)
        {
            var request = CreateAllegroApiRequest($"offers/listing?offer.id={productId}");

            var response = await SendAllegroApiRequest(request);

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var responseString = await response.Content.ReadAsStringAsync();

            var productResponse = DeserializeJson<ProductResponse>(responseString);

            var product = CreateProductFromResponse(productResponse);

            return Ok(product);
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
    }

    public class ProductResponse
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("category")] public Category Category { get; set; }

        [JsonProperty("sellingMode")] public SellingMode SellingMode { get; set; }

        [JsonProperty("images")] public PictureResponse Images { get; set; }

        [JsonProperty("parameters")] public ParametersResponse Parameters { get; set; }

        [JsonProperty("seller")] public Vendor Seller { get; set; }

        [JsonProperty("stock")] public Stock Stock { get; set; }

        [JsonProperty("condition")] public Condition Condition { get; set; }

        [JsonProperty("ean")] public string Ean { get; set; }
    }

    public class PictureResponse
    {
        [JsonProperty("pictures")] public Pictures Pictures { get; set; }
    }

    public class Pictures
    {
        [JsonProperty("data")] public List<Image> Data { get; set; }
    }

    public class ParametersResponse
    {
        [JsonProperty("parameters")] public Parameters Parameters { get; set; }
    }

    public class Parameters
    {
        [JsonProperty("data")] public List<Attribute> Data { get; set; }
    }


    public class Product
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("category")] public Category Category { get; set; }

        [JsonProperty("sellingMode")] public SellingMode SellingMode { get; set; }

        [JsonProperty("images")] public List<Image> Images { get; set; }

        [JsonProperty("attributes")] public List<Attribute> Attributes { get; set; }

        [JsonProperty("vendor")] public Vendor Vendor { get; set; }

        [JsonProperty("stock")] public Stock Stock { get; set; }

        [JsonProperty("condition")] public Condition Condition { get; set; }

        [JsonProperty("ean")] public string Ean { get; set; }
    }

    public class Category
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }
    }

    public class SellingMode
    {
        [JsonProperty("format")] public string Format { get; set; }

        [JsonProperty("price")] public Price Price { get; set; }

        [JsonProperty("popularity")] public int Popularity { get; set; }
    }

    public class Price
    {
        [JsonProperty("amount")] public double Amount { get; set; }

        [JsonProperty("currency")] public string Currency { get; set; }
    }

    public class Image
    {
        [JsonProperty("url")] public string Url { get; set; }
    }

    public class Attribute
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("values")] public List<AttributeValue> Values { get; set; }
    }

    public class AttributeValue
    {
        [JsonProperty("value")] public string Value { get; set; }
    }

    public class Vendor
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("company")] public bool IsCompany { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("address")] public Address Address { get; set; }

        [JsonProperty("phone")] public string Phone { get; set; }

        [JsonProperty("email")] public string Email { get; set; }

        [JsonProperty("rating")] public Rating Rating { get; set; }
    }

    public class Address
    {
        [JsonProperty("city")] public string City { get; set; }

        [JsonProperty("postcode")] public string Postcode { get; set; }

        [JsonProperty("country")] public string Country { get; set; }

        [JsonProperty("street")] public string Street { get; set; }

        [JsonProperty("buildingNumber")] public string BuildingNumber { get; set; }
    }

    public class Rating
    {
        [JsonProperty("average")] public double Average { get; set; }

        [JsonProperty("count")] public int Count { get; set; }

        [JsonProperty("proportion")] public Dictionary<string, double> Proportion { get; set; }
    }

    public class Stock
    {
        [JsonProperty("unit")] public string Unit { get; set; }

        [JsonProperty("available")] public int Available { get; set; }

        [JsonProperty("sold")] public int Sold { get; set; }
    }

    public class Condition
    {
        [JsonProperty("condition")] public string Type { get; set; }
    }

    public class SalesResponse
    {
        [JsonProperty("sales")] public List<SaleResponse> Sales { get; set; }
    }

    public class SaleResponse
    {
        [JsonProperty("date")] public string Date { get; set; }

        [JsonProperty("quantity")] public int Quantity { get; set; }
    }

    public class Sale
    {
        public string Date { get; set; }

        public int Quantity { get; set; }
    }
}