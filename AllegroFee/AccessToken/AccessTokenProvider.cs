using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

public class AccessTokenProvider : IAccessTokenProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _tokenUrl;

    private string _accessToken;
    private DateTime _accessTokenExpiration;

    public AccessTokenProvider(HttpClient httpClient, string clientId, string clientSecret, string tokenUrl)
    {
        _httpClient = httpClient;
        _clientId = clientId;
        _clientSecret = clientSecret;
        _tokenUrl = tokenUrl;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (_accessTokenExpiration < DateTime.UtcNow)
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _tokenUrl);
            var authorizationHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}")));
            tokenRequest.Headers.Authorization = authorizationHeader;
            tokenRequest.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var tokenResponse = await _httpClient.SendAsync(tokenRequest);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get access token. StatusCode={tokenResponse.StatusCode} Reason={tokenResponse.ReasonPhrase}");
            }

            var tokenResult = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<AccessTokenData>(tokenResult);
            _accessToken = tokenData.AccessToken;
            _accessTokenExpiration = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);
        }

        return _accessToken;
    }

    private class AccessTokenData
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}