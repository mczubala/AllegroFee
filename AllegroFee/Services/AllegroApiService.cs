using AllegroFee.Interfaces;

public class AllegroApiService : IAllegroApiService
{
    private readonly string AllegroApiBaseUrl;

    public AllegroApiService(IConfiguration configuration)
    {
        AllegroApiBaseUrl = configuration.GetValue<string>("AllegroApiBaseUrl");
    }

    public HttpRequestMessage CreateAllegroApiRequest(string relativeUrl, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{AllegroApiBaseUrl}/{relativeUrl}");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        return request;
    }
}