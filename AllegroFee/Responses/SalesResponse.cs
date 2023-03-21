using Newtonsoft.Json;

namespace AllegroFee.Responses;

public class SalesResponse
{
    [JsonProperty("sales")] public List<SaleResponse> Sales { get; set; }
}