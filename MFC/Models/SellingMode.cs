using Newtonsoft.Json;

namespace MFC.Models;

public class SellingMode
{
    [JsonProperty("price")]
    public Price Price { get; set; }
}