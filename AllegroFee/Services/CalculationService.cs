using System.Net;
using AllegroFee.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace AllegroFee.Services;

public class CalculationService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly string _allegroApiBaseUrl;
    
    public CalculationService(IHttpClientFactory clientFactory, IAccessTokenProvider accessTokenProvider, IConfiguration configuration)
    {
        _clientFactory = clientFactory;
        _accessTokenProvider = accessTokenProvider;
        _allegroApiBaseUrl = configuration.GetValue<string>("AllegroApiBaseUrl");
    }
    
    // public async Task<> GetFeeForOfferIdAsync(string offerId)
    // {
    //
    // }

    public async Task<JObject> GetBillingByOfferIdAsync(string offerId)
    {
        var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
        var request = AllegroApiService.CreateAllegroApiRequest(_allegroApiBaseUrl,$"billing/billing-entries?offer.id={offerId}", accessToken);
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
        var test = ParseBillingEntries(responseString);
        var responseObject = JObject.Parse(responseString);
        return responseObject;
    }
    public static List<BillingEntry> ParseBillingEntries(string jsonString)
    {
        JObject jsonResponse = JObject.Parse(jsonString);
        JArray entriesArray = (JArray)jsonResponse["billingEntries"];

        // Newtonsoft.Json library is used for deserialization
        var billingEntries = entriesArray.ToObject<List<BillingEntry>>();

        return billingEntries;
    }

}