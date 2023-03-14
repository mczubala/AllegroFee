using Newtonsoft.Json;

namespace AllegroFee.Models;

public class Rating
{
    [JsonProperty("average")] public double Average { get; set; }

    [JsonProperty("count")] public int Count { get; set; }

    [JsonProperty("proportion")] public Dictionary<string, double> Proportion { get; set; }
}