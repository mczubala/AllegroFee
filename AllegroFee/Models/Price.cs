using Newtonsoft.Json;

namespace AllegroFee.Models;

public class Price
{
    [JsonProperty("amount")] public double Amount { get; set; }

    [JsonProperty("currency")] public string Currency { get; set; }
}