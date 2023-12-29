using Newtonsoft.Json;

namespace MFC.AccessToken;

public class AccessTokenData
{
    [JsonProperty("access_token")] public string AccessToken { get; set; }
    [JsonProperty("expires_in")] public int ExpiresIn { get; set; } 
    [JsonProperty("refresh_token")] public string RefreshToken { get; set; }
}