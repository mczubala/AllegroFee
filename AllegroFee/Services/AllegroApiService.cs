using System.Net;
using System.Net.Http.Headers;
using AllegroFee.Interfaces;
using AllegroFee.Models;
using Newtonsoft.Json.Linq;

public class AllegroApiService : IAllegroApiService
{
    private readonly string AllegroApiBaseUrl;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IAccessTokenProvider _accessTokenProvider;
    public AllegroApiService(IHttpClientFactory clientFactory, IAccessTokenProvider accessTokenProvider, IConfiguration configuration)
    {
        AllegroApiBaseUrl = configuration.GetValue<string>("AllegroApiBaseUrl");
        _clientFactory = clientFactory;
        _accessTokenProvider = accessTokenProvider;
    }

    public HttpRequestMessage CreateAllegroApiRequest(string relativeUrl, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{AllegroApiBaseUrl}/{relativeUrl}");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        return request;
    }
    
    public async Task<HttpResponseMessage> SendAllegroApiRequest(HttpRequestMessage request)
    {
        var client = _clientFactory.CreateClient();
        return await client.SendAsync(request);
    }
    
    public async Task<Order> GetOrderByIdAsync(string orderId)
    {
        try
        {
            var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
            var request = CreateAllegroApiRequest($"order/checkout-forms/{orderId}", accessToken);
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ArgumentException("The requested resource was not found.");
                }

                throw new BadHttpRequestException(
                    $"Failed to get order information. StatusCode={response.StatusCode} Reason={response.ReasonPhrase}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = ParseOrder(responseString);
            return responseObject;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task<List<BillingEntry>> GetBillingByOfferIdAsync(string offerId)
    {
        var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
        var request = CreateAllegroApiRequest($"billing/billing-entries?offer.id={offerId}", accessToken);
        var client = _clientFactory.CreateClient();
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ArgumentException("The requested resource was not found.");
            }

            throw new BadHttpRequestException(
                $"Failed to get billing information. StatusCode={response.StatusCode} Reason={response.ReasonPhrase}");
        }
        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JObject.Parse(responseString);
        
        return ParseBillingEntries(responseObject.ToString());
    }
    
    public async Task<JObject> GetCurrentUserDetailsAsync()
    {
        var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
        var request = CreateAllegroApiRequest("me", accessToken);
        
        var response = await SendAllegroApiRequest(request);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var userDetails = JObject.Parse(responseString);

        return userDetails;
    }
    public async Task<JObject> GetAllBillingEntriesAsync()
    {
        var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
        var request = CreateAllegroApiRequest("billing/billing-entries", accessToken);
        var client = _clientFactory.CreateClient();
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ArgumentException("The requested resource was not found.");
            }

            throw new BadHttpRequestException(
                $"Failed to get billing information. StatusCode={response.StatusCode} Reason={response.ReasonPhrase}");
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JObject.Parse(responseString);
        return responseObject;
    }
    #region Private methods

    private static List<BillingEntry> ParseBillingEntries(string jsonString)
    {
        try
        {
            JObject jsonResponse = JObject.Parse(jsonString);
            JArray entriesArray = (JArray)jsonResponse["billingEntries"];

            // Newtonsoft.Json library is used for deserialization
            var billingEntries = entriesArray.ToObject<List<BillingEntry>>();

            return billingEntries;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    private static Order ParseOrder(string jsonString)
    {
        try
        {
            JObject jsonResponse = JObject.Parse(jsonString);
            var order = jsonResponse.ToObject<Order>();

            return order;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    #endregion
}