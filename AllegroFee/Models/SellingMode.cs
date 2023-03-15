using Newtonsoft.Json;

namespace AllegroFee.Models;

public class SellingMode
{
    [JsonProperty("price")]
    public Price Price { get; set; }
}