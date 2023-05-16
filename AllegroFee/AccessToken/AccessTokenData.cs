using Newtonsoft.Json;

namespace AllegroFee.AccessToken;

public class AccessTokenData
{
    [JsonProperty("access_token")] public string AccessToken { get; set; }

    [JsonProperty("expires_in")] public int ExpiresIn { get; set; }
}