using Newtonsoft.Json;

namespace AllegroFee.Models;

public class Pictures
{
    [JsonProperty("data")] public List<Image> Data { get; set; }
}