using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AccessTokenProvider : IAccessTokenProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _tokenUrl;
    private readonly string _authorizationEndpoint;
    private readonly string _redirectUri = "http://localhost:8000";
    private string _accessToken;
    private DateTime _accessTokenExpiration;
    private readonly string _tokenFileName = "access_token.json";

    public AccessTokenProvider(HttpClient httpClient, string clientId, string clientSecret, string tokenUrl,
        string authorizationEndpoint)
    {
        _httpClient = httpClient;
        _clientId = clientId;
        _clientSecret = clientSecret;
        _tokenUrl = tokenUrl;
        _authorizationEndpoint = authorizationEndpoint;
    }

    public async Task<string> GetAccessForApplicationTokenAsync()
    {
        if (_accessTokenExpiration < DateTime.UtcNow)
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _tokenUrl);
            var authorizationHeader = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}")));
            tokenRequest.Headers.Authorization = authorizationHeader;
            tokenRequest.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8,
                "application/x-www-form-urlencoded");

            var tokenResponse = await _httpClient.SendAsync(tokenRequest);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Failed to get access token. StatusCode={tokenResponse.StatusCode} Reason={tokenResponse.ReasonPhrase}");
            }

            var tokenResult = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<AccessTokenData>(tokenResult);
            _accessToken = tokenData.AccessToken;
            _accessTokenExpiration = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);
        }

        return _accessToken;
    }
    public async Task<string> GetAccessForUserTokenAsync()
    {
        if (_accessToken != null && DateTime.UtcNow < _accessTokenExpiration)
        {
            (string accessToken, DateTime expirationTime) = await LoadAccessTokenFromFileAsync();

            if (accessToken != null && DateTime.UtcNow < expirationTime)
            {
                _accessToken = accessToken;
                _accessTokenExpiration = expirationTime;
                return _accessToken;
            }
        }
        string codeVerifier = GenerateCodeVerifier();
        string codeChallenge = GenerateCodeChallenge(codeVerifier);
        string authorizationUrl = $"https://allegro.pl.allegrosandbox.pl/auth/oauth/authorize?response_type=code&client_id={_clientId}&redirect_uri={Uri.EscapeDataString(_redirectUri)}&code_challenge_method=S256&code_challenge={codeChallenge}";

        Console.WriteLine("Please open the following URL in your browser and grant the required permissions:");
        Console.WriteLine(authorizationUrl);
        Console.WriteLine("After granting permissions, you will be redirected to the specified redirect URI. Please enter the 'code' parameter from the URL:");

        string authorizationCode = Console.ReadLine();

        using HttpClient httpClient = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://allegro.pl.allegrosandbox.pl/auth/oauth/token");

        request.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("redirect_uri", _redirectUri),
            new KeyValuePair<string, string>("code_verifier", codeVerifier)
        });

        HttpResponseMessage response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync();
        JObject jsonResponse = JObject.Parse(responseContent);

        // Store the access token and its expiration time
        _accessToken = jsonResponse["access_token"].ToString();
        int expiresIn = jsonResponse["expires_in"].ToObject<int>();
        _accessTokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn);
        await SaveAccessTokenToFileAsync(_accessToken, _accessTokenExpiration);
        return _accessToken;
    }

    private static string GenerateCodeVerifier()
    {
        using RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] randomBytes = new byte[80];
        rng.GetBytes(randomBytes);

        return Base64UrlEncode(randomBytes);
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));

        return Base64UrlEncode(challengeBytes);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private async Task SaveAccessTokenToFileAsync(string accessToken, DateTime expirationTime)
    {
        var data = new Dictionary<string, string>
        {
            { "accessToken", accessToken },
            { "expirationTime", expirationTime.ToString("o") }
        };
        string json = JsonConvert.SerializeObject(data);

        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AccessToken.json");
        await File.WriteAllTextAsync(filePath, json);
    }
    
    private async Task<(string accessToken, DateTime expirationTime)> LoadAccessTokenFromFileAsync()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AccessToken.json");

        if (!File.Exists(filePath))
        {
            return (null, DateTime.MinValue);
        }

        string json = await File.ReadAllTextAsync(filePath);
        var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        string accessToken = data["accessToken"];
        DateTime expirationTime = DateTime.Parse(data["expirationTime"], null, DateTimeStyles.RoundtripKind);

        return (accessToken, expirationTime);
    }


    public class TokenErrorData
    {
        [JsonProperty("error")] public string Error { get; set; }

        [JsonProperty("error_description")] public string ErrorDescription { get; set; }

        [JsonProperty("error_uri")] public string ErrorUri { get; set; }
    }
    
    public class AccessTokenData
    {
        [JsonProperty("access_token")] public string AccessToken { get; set; }

        [JsonProperty("expires_in")] public int ExpiresIn { get; set; }
    }
}