using Newtonsoft.Json;

namespace MFC.Models;

public class AccessTokenResult
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
}