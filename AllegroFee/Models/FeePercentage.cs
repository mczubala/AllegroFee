using Newtonsoft.Json;

namespace AllegroFee.Models;

public class FeePercentage
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("amount")]
    public decimal Amount { get; set; }
}