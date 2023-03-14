using Newtonsoft.Json;

namespace AllegroFee.Models;

public class Condition
{
    [JsonProperty("condition")] public string Type { get; set; }
}