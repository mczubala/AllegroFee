namespace AllegroFee.Interfaces;

public interface IAllegroApiService
{ 
    HttpRequestMessage CreateAllegroApiRequest(string relativeUrl, string accessToken);
}
