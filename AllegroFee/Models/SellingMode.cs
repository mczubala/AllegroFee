using Newtonsoft.Json;

namespace AllegroFee.Models;

public class SellingMode
{
    [JsonProperty("format")] public string Format { get; set; }

    [JsonProperty("price")] public Price Price { get; set; }

    [JsonProperty("popularity")] public int Popularity { get; set; }
}