using Newtonsoft.Json;

namespace MFC.Models;

public class Image
{
    [JsonProperty("url")] public string Url { get; set; }
}