using AllegroFee.Models;
using Newtonsoft.Json;

namespace AllegroFee.Responses;

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