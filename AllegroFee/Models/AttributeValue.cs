using Newtonsoft.Json;

namespace AllegroFee.Models;

public class AttributeValue
{
    [JsonProperty("value")] public string Value { get; set; }
}