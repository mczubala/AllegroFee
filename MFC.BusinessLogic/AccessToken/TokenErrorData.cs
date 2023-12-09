using Newtonsoft.Json;

namespace MFC.AccessToken;

public class TokenErrorData
{
    [JsonProperty("error")] public string Error { get; set; }

    [JsonProperty("error_description")] public string ErrorDescription { get; set; }

    [JsonProperty("error_uri")] public string ErrorUri { get; set; }
}