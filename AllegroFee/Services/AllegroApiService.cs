public static class AllegroApiService
{
    public static HttpRequestMessage CreateAllegroApiRequest(string AllegroApiBaseUrl, string relativeUrl, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{AllegroApiBaseUrl}/{relativeUrl}");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        return request;
    }
}