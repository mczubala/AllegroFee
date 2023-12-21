using Newtonsoft.Json;

namespace MFC.Models;

public class Price
{
    [JsonProperty("amount")] public decimal Amount { get; set; }

    [JsonProperty("currency")] public string Currency { get; set; }
}