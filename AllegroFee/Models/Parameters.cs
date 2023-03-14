using Newtonsoft.Json;

namespace AllegroFee.Models;

public class Parameters
{
    [JsonProperty("data")] public List<Attribute> Data { get; set; }
}