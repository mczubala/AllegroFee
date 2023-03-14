using Newtonsoft.Json;

namespace AllegroFee.Responses;

public class SaleResponse
{
    [JsonProperty("date")] public string Date { get; set; }

    [JsonProperty("quantity")] public int Quantity { get; set; }
}