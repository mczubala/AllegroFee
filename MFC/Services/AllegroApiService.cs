using System.Net;
using FluentValidation.Results;
using MFC.Configurations;
using MFC.Interfaces;
using MFC.Models;
using MFC.Responses;
using MFC.Validators;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace MFC.Services;

public class AllegroApiService : IAllegroApiService
{
    private readonly AllegroApiSettings _allegroApiSettings;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IAccessTokenProvider _accessTokenProvider;
    public AllegroApiService(IHttpClientFactory clientFactory, IAccessTokenProvider accessTokenProvider, IOptions<AllegroApiSettings> allegroApiSettings)
    {
        _allegroApiSettings = allegroApiSettings.Value;
        _clientFactory = clientFactory;
        _accessTokenProvider = accessTokenProvider;
    }
    
    public HttpRequestMessage CreateAllegroApiRequest(string relativeUrl, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_allegroApiSettings.AllegroApiBaseUrl}/{relativeUrl}");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        return request;
    }

    public async Task<HttpResponseMessage> SendAllegroApiRequest(HttpRequestMessage request)
    {
        var client = _clientFactory.CreateClient();
        HttpResponseMessage response;

        try
        {
            response = await client.SendAsync(request);
        
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error sending request to Allegro API: {response.StatusCode}\n{errorContent}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
        }

        return response;
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
            var order = ParseOrder(responseString);

            // Validate the order object
            ValidationResult validationResult;
            OrderValidator validator = new OrderValidator();
            validationResult = validator.Validate(order);

            if (!validationResult.IsValid) 
            {
                throw new ApiDataException("Invalid order data received from API.", 1001);
            }

            return order;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task<ExternalApiResponse<List<BillingEntry>>> GetBillingByOfferIdAsync(string offerId)
    {
        try
        {
            var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
            var request = CreateAllegroApiRequest($"billing/billing-entries?offer.id={offerId}", accessToken);
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new ExternalApiResponse<List<BillingEntry>>(
                        "The requested resource was not found.",HttpStatusCode.NotFound);
                }

                return new ExternalApiResponse<List<BillingEntry>>(
                    $"Failed to get billing information. StatusCode={response.StatusCode} Reason={response.ReasonPhrase} Content={errorContent}",
                    response.StatusCode);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var billingEntries = ParseBillingEntries(responseString);
            return new ExternalApiResponse<List<BillingEntry>>(billingEntries);
        }
        catch (HttpRequestException ex)
        {
            return new ExternalApiResponse<List<BillingEntry>>(ex.Message, HttpStatusCode.BadGateway);
        }
        catch (Exception ex)
        {
            return new ExternalApiResponse<List<BillingEntry>>(ex.Message, HttpStatusCode.InternalServerError);
        }
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
    
    public async Task<ExternalApiResponse<List<BillingEntry>>> GetAllBillingEntriesAsync()
    {
        try
        {
            var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
            var request = CreateAllegroApiRequest("billing/billing-entries", accessToken);
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new ExternalApiResponse<List<BillingEntry>>(
                    $"Failed to get billing information. StatusCode={response.StatusCode} Reason={response.ReasonPhrase} Content={errorContent}",
                    response.StatusCode);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = ParseBillingEntries(responseString);
            return new ExternalApiResponse<List<BillingEntry>>(responseObject);
        }
        catch (HttpRequestException ex)
        {
            return new ExternalApiResponse<List<BillingEntry>>(ex.Message, HttpStatusCode.BadGateway);
        }
        catch (Exception ex)
        {
            return new ExternalApiResponse<List<BillingEntry>>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }

    #region Private methods

    private static List<BillingEntry> ParseBillingEntries(string jsonString)
    {
        try
        {
            JObject jsonResponse = JObject.Parse(jsonString);
            JArray entriesArray = (JArray)jsonResponse["billingEntries"];

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