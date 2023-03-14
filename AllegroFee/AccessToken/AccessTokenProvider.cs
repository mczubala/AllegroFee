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
            return _accessToken;
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

    // public async Task<string> GetAccessTokenForUserAsync()
    // {
    //     // Authorization endpoint for user authentication
    //     var authorizationEndpoint = _authorizationEndpoint;
    //
    //     // Token endpoint to exchange authorization code for an access token
    //     var tokenEndpoint = _tokenUrl;
    //
    //     // Your client ID and secret obtained from the Allegro Developer Dashboard
    //     var clientId = _clientId;
    //     var clientSecret = _clientSecret;
    //
    //     // The redirect URI for your application to receive the authorization code
    //     var redirectUri = "http://localhost:8000";
    //
    //     // The scope of the access being requested
    //     var scope = "allegro:api:sale:offers:read";
    //
    //     // Create a new HttpClient to send requests to the authorization and token endpoints
    //     var httpClient = new HttpClient();
    //
    //     // Create a new HttpRequestMessage to send an authorization request to the authorization endpoint
    //     var authorizationRequest = new HttpRequestMessage(HttpMethod.Get, authorizationEndpoint + "?response_type=code" +
    //                                                                       $"&client_id={clientId}&redirect_uri={redirectUri}&scope={scope}");
    //     // Send the authorization request to the authorization endpoint
    //     var authorizationResponse = await httpClient.SendAsync(authorizationRequest);
    //     
    //     // If the response status code is not OK, something went wrong and the user cannot be authorized
    //     if (!authorizationResponse.IsSuccessStatusCode)
    //     {
    //         //throw new Exception("Failed to authorize user.");
    //     }
    //     
    //     // Read the authorization response content to get the authorization code from the query string
    //     var authorizationResponseContent = await authorizationResponse.Content.ReadAsStringAsync();
    //     var authorizationCode = HttpUtility.ParseQueryString(authorizationResponseContent)["code"];
    //
    //     // Create a new HttpRequestMessage to send a token request to the token endpoint
    //     var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
    //     tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
    //     {
    //         { "grant_type", "authorization_code" },
    //         { "code", authorizationCode },
    //         { "redirect_uri", redirectUri }
    //     });
    //
    //     // Set the Basic authentication header with the client ID and secret
    //     var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));
    //     tokenRequest.Headers.Authorization = authHeader;
    //
    //     // Send the token request to the token endpoint
    //     var tokenResponse = await httpClient.SendAsync(tokenRequest);
    //
    //     // If the response status code is not OK, something went wrong and the access token cannot be obtained
    //     if (!tokenResponse.IsSuccessStatusCode)
    //     {
    //         throw new Exception("Failed to get access token.");
    //     }
    //
    //     // Read the token response content to get the access token
    //     var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
    //     var tokenData = JsonConvert.DeserializeObject<AccessTokenData>(tokenResponseContent);
    //
    //     return tokenData.AccessToken;
    // }

    public async Task<string> GetAccessTokenUsingDeviceFlowAsync()
    {
        // Device flow endpoint to obtain device flow code
        var deviceFlowEndpoint = "https://allegro.pl.allegrosandbox.pl/auth/oauth/device";

        // Token endpoint to exchange device flow code for an access token
        var tokenEndpoint = _tokenUrl;

        // Your client ID obtained from the Allegro Developer Dashboard
        var clientId = _clientId;
        var clientSecret = _clientSecret;
        // The scope of the access being requested
        var scope = "allegro:api:sale:offers:read";

        // Create a new HttpClient to send requests to the device flow and token endpoints
        var httpClient = new HttpClient();

        // Create a new HttpRequestMessage to send a device flow request to the device flow endpoint
        var deviceFlowRequest = new HttpRequestMessage(HttpMethod.Post, deviceFlowEndpoint);

        // Set the Authorization header with the client ID and secret
        var authHeader = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));
        deviceFlowRequest.Headers.Authorization = authHeader;

        // Set the Content-Type header
        deviceFlowRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "scope", scope }
        });
        deviceFlowRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        // Send the device flow request to the device flow endpoint
        var deviceFlowResponse = await httpClient.SendAsync(deviceFlowRequest);

        // If the response status code is not OK, something went wrong and the device code cannot be obtained
        if (!deviceFlowResponse.IsSuccessStatusCode)
        {
            throw new Exception("Failed to obtain device code.");
        }

        // Read the device flow response content to get the device code and verification URL
        var deviceFlowResponseContent = await deviceFlowResponse.Content.ReadAsStringAsync();
        var deviceFlowData = JsonConvert.DeserializeObject<DeviceFlowData>(deviceFlowResponseContent);

        // Display the verification URL and user code to the user
        Console.WriteLine(
            $"Please visit {deviceFlowData.VerificationUri} and enter the code {deviceFlowData.UserCode}");

        // Poll the token endpoint until an access token is obtained
        var accessToken = string.Empty;
        var interval = deviceFlowData.Interval;

        while (accessToken == string.Empty)
        {
            // Wait for the specified interval before polling again
            await Task.Delay(interval * 1000);

            // Create a new HttpRequestMessage to send a token request to the token endpoint
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" },
                { "device_code", deviceFlowData.DeviceCode }
            });

            // Set the Basic authentication header with the client ID and secret
            tokenRequest.Headers.Authorization = authHeader;

            // Send the token request to the token endpoint
            var tokenResponse = await httpClient.SendAsync(tokenRequest);

            // If the response status code is not OK, something went wrong and the access token cannot be obtained
            if (!tokenResponse.IsSuccessStatusCode)
            {
                // If the response status code is 400 (Bad Request) and the error is "authorization_pending", continue polling
                if (tokenResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
                    var tokenErrorData = JsonConvert.DeserializeObject<TokenErrorData>(tokenResponseContent);

                    if (tokenErrorData.Error == "authorization_pending")
                    {
                        continue;
                    }
                }

                throw new Exception("Failed to get access token.");
            }


            // Read the token response content to get the access token
            var tokenResponseContent2 = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<AccessTokenData>(tokenResponseContent2);

            return tokenData.AccessToken;
        }

        return string.Empty;
    }

    public class TokenErrorData
    {
        [JsonProperty("error")] public string Error { get; set; }

        [JsonProperty("error_description")] public string ErrorDescription { get; set; }

        [JsonProperty("error_uri")] public string ErrorUri { get; set; }
    }

    public class DeviceFlowData
    {
        [JsonProperty("device_code")] public string DeviceCode { get; set; }

        [JsonProperty("user_code")] public string UserCode { get; set; }

        [JsonProperty("verification_uri")] public string VerificationUri { get; set; }

        [JsonProperty("expires_in")] public int ExpiresIn { get; set; }

        [JsonProperty("interval")] public int Interval { get; set; }

        [JsonProperty("verification_uri_complete")]
        public string VerificationUriComplete { get; set; }
    }

    public class AccessTokenData
    {
        [JsonProperty("access_token")] public string AccessToken { get; set; }

        [JsonProperty("expires_in")] public int ExpiresIn { get; set; }
    }
}