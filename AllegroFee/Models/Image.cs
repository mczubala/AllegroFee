using Newtonsoft.Json;

namespace AllegroFee.Models;

public class Image
{
    [JsonProperty("url")] public string Url { get; set; }
}