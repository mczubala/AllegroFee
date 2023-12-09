using Newtonsoft.Json;

namespace MFC.Models;

public class Stock
{
    [JsonProperty("unit")] public string Unit { get; set; }

    [JsonProperty("available")] public int Available { get; set; }

    [JsonProperty("sold")] public int Sold { get; set; }
}