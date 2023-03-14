using AllegroFee.Models;
using Newtonsoft.Json;
using YourProject.Controllers;

namespace AllegroFee.Responses;

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