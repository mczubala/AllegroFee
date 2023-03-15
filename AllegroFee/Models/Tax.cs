using Newtonsoft.Json;

namespace AllegroFee.Models;

public class Tax
{
    [JsonProperty("percentage")]
    public string Percentage { get; set; }
    [JsonProperty("rate")]
    public string Rate { get; set; }
    [JsonProperty("subject")]
    public string Subject { get; set; }
}