using Newtonsoft.Json;

namespace AllegroFee.Models;

public class AccessTokenResult
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
}