using Newtonsoft.Json;
using AllegroFee.Controllers;
using Attribute = System.Attribute;

namespace AllegroFee.Models;

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
    public List<Promotion> Promotions { get; set; }

}