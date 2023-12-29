using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Caching;
using MFC.AccessToken;
using MFC.DataAccessLayer.Entities;
using MFC.DataAccessLayer.Repository;
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
    private readonly IMfcDbRepository _mfcDbRepositoryFactory;
    
    public AccessTokenProvider(HttpClient httpClient, string clientId, string clientSecret, string tokenUrl,
        string authorizationEndpoint, IMfcDbRepository mfcDbRepositoryFactory)
    {
        _httpClient = httpClient;
        _clientId = clientId;
        _clientSecret = clientSecret;
        _tokenUrl = tokenUrl;
        _authorizationEndpoint = authorizationEndpoint;
        _mfcDbRepositoryFactory = mfcDbRepositoryFactory;
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

        return tokenData.AccessToken;
    }
    public async Task<string> GetAccessForUserTokenAsync()
    {
        // Try to get the access token from cache
        _accessToken = GetAccessTokenFromCache();
        if (_accessToken != null)
        {
            return _accessToken;
        }
        
        // If the token is not in cache or it is expired, try to get it from the database
        if (_accessToken == null || DateTime.UtcNow >= _accessTokenExpiration)
        {
            var allegroAccessToken = await _mfcDbRepositoryFactory.GetAllegroAccessTokenByClientIdAsync(_clientId);

            // If token is found in the database and it is not expired, return it
            if (allegroAccessToken != null && DateTime.UtcNow < allegroAccessToken.ExpiresIn)
            {
                _accessToken = allegroAccessToken.AccessToken;
                _accessTokenExpiration = allegroAccessToken.ExpiresIn;
                SaveAccessTokenToCache(_accessToken, _accessTokenExpiration);
                return _accessToken;
            }

            // If the token from the database is expired, refresh it
            if (allegroAccessToken != null && DateTime.UtcNow >= allegroAccessToken.ExpiresIn)
            {
                _accessToken = await RefreshUserTokenAsync(allegroAccessToken.RefreshToken);
                return _accessToken;
            }
        }

        // If the token is not available in cache or database, initiate the authorization process
        return await InitiateAuthorizationProcess();
    }

    private async Task<string> InitiateAuthorizationProcess()
    {
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

        _accessToken = jsonResponse["access_token"].ToString();
        int expiresIn = jsonResponse["expires_in"].ToObject<int>();
        _accessTokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn);
        SaveAccessTokenToCache(_accessToken, _accessTokenExpiration);

        var refreshToken = jsonResponse["refresh_token"].ToString();

        var allegroAccessToken = new AllegroAccessToken(_clientId, _accessToken)
        {
            RefreshToken = refreshToken,
            ExpiresIn = _accessTokenExpiration
        };
    
        _mfcDbRepositoryFactory.AddAllegroAccessToken(allegroAccessToken);
        await _mfcDbRepositoryFactory.SaveChangesAsync();
        return _accessToken;
    }


    public async Task<string> RefreshUserTokenAsync(string refreshToken)
    {
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _tokenUrl);
        var authorizationHeader = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}")));
        tokenRequest.Headers.Authorization = authorizationHeader;

        tokenRequest.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
            new KeyValuePair<string, string>("redirect_uri", _redirectUri)
        });

        var tokenResponse = await _httpClient.SendAsync(tokenRequest);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Failed to refresh token. StatusCode={tokenResponse.StatusCode} Reason={tokenResponse.ReasonPhrase}");
        }

        var tokenResult = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = JsonConvert.DeserializeObject<AccessTokenData>(tokenResult);

        // Update the access token, expiration time, and refresh token
        _accessToken = tokenData.AccessToken;
        _accessTokenExpiration = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);

        var newRefreshToken = tokenData.RefreshToken;

        // Update the stored refresh token
        var allegroAccessToken = await _mfcDbRepositoryFactory.GetAllegroAccessTokenByClientIdAsync(_clientId);
        allegroAccessToken.AccessToken = _accessToken;
        allegroAccessToken.RefreshToken = newRefreshToken;
        allegroAccessToken.ExpiresIn = _accessTokenExpiration;

        await _mfcDbRepositoryFactory.UpdateAllegroAccessTokenAsync(allegroAccessToken);
        await _mfcDbRepositoryFactory.SaveChangesAsync();

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