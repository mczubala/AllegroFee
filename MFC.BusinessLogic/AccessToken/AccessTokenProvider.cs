using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Caching;
using MFC.AccessToken;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AccessTokenProvider : IAccessTokenProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _tokenUrl;
    private readonly string _redirectUri = "http://localhost:8000";
    private string _accessToken;
    private DateTime _accessTokenExpiration;
    private MemoryCache _cache = new MemoryCache("AccessTokenCache");
    private readonly string _authorizationEndpoint;

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
            //_accessToken = tokenData.AccessToken;
            //_accessTokenExpiration = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);

        return tokenData.AccessToken;
    }
    public async Task<string> GetAccessForUserTokenAsync()
    {
        _accessToken = GetAccessTokenFromCache();

        if (_accessToken != null && DateTime.UtcNow < _accessTokenExpiration)
        {
            return _accessToken;
        }
        
        string codeVerifier = GenerateCodeVerifier();
        string codeChallenge = GenerateCodeChallenge(codeVerifier);
        string authorizationUrl = $"{_authorizationEndpoint}?response_type=code&client_id={_clientId}&redirect_uri={Uri.EscapeDataString(_redirectUri)}&code_challenge_method=S256&code_challenge={codeChallenge}";

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
        SaveAccessTokenToCache(_accessToken, _accessTokenExpiration);
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
    
    private void SaveAccessTokenToCache(string accessToken, DateTime expirationTime)
    {
        CacheItemPolicy policy = new CacheItemPolicy
        {
            AbsoluteExpiration = expirationTime
        };
        _cache.Set("AccessToken", accessToken, policy);
    }

    private string GetAccessTokenFromCache()
    {
        return _cache.Get("AccessToken") as string;
    }

}