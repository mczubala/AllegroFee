using Newtonsoft.Json;
using YourProject.Controllers;

namespace AllegroFee.Responses;

public class SalesResponse
{
    [JsonProperty("sales")] public List<SaleResponse> Sales { get; set; }
}