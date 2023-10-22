using System.Globalization;
using System.Net;
using AllegroFee.Interfaces;
using AllegroFee.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AllegroFee.Services;

public class CalculationService : ICalculationService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IAllegroApiService _allegroApiService;

    public CalculationService(IHttpClientFactory clientFactory, IAccessTokenProvider accessTokenProvider, IAllegroApiService allegroApiService)
    {
        _clientFactory = clientFactory;
        _accessTokenProvider = accessTokenProvider;
        _allegroApiService = allegroApiService;
    }
    public async Task<JObject> GetAllBillingEntriesAsync()
    {
        var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
        var request = _allegroApiService.CreateAllegroApiRequest("billing/billing-entries", accessToken);
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

    public async Task<JObject> GetBillingByOfferIdAsync(string offerId)
    {
        var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
        var request = _allegroApiService.CreateAllegroApiRequest($"billing/billing-entries?offer.id={offerId}", accessToken);
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
        //calculation algorithm
        var billingEntries = ParseBillingEntries(responseString);
        // Group by order id
        var groupedEntries = billingEntries
            .Where(x => x.Order != null) // exclude entries with null Order
            .GroupBy(x => x.Order.Id);

        // Calculate sum for each group
        var sumResult = groupedEntries.Select(group => new
        {
            OrderId = group.Key,
            TotalAmount = group.Sum(item => decimal.Parse(item.Value.Amount, CultureInfo.InvariantCulture))
        });
        
        var uniqueOfferIds = GetListOfUniqueOrderDataID(billingEntries);
        List<JObject> orders = new List<JObject>();
        foreach (var uniqueOfferId in uniqueOfferIds)
        {
             orders.Add(GetOrderByIdAsync(uniqueOfferId).Result); 
        }
        List<Order> ordersList = new List<Order>();
        foreach (var order in orders)
        {
            Order orderObj = JsonConvert.DeserializeObject<Order>(order.ToString());
            ordersList.Add(orderObj);
        }
        
        ProductFee productFee = new ProductFee();
        productFee.ProductId = offerId;
        decimal totalFee = 0;
        var firstsumresult = sumResult.FirstOrDefault();
        var xxx = ordersList.Where(x => x.Id == firstsumresult.OrderId).FirstOrDefault();
        var quantity = xxx.LineItems.Where(x => x.Offer.Id == offerId).FirstOrDefault().Quantity;
        var price = xxx.LineItems.Where(x => x.Offer.Id == offerId).FirstOrDefault().Price.AmountValue;
        totalFee = firstsumresult.TotalAmount;
        var percentFee = totalFee / (quantity * decimal.Parse(price, CultureInfo.InvariantCulture));
        //
        
        var responseObject = JObject.Parse(responseString);
        return responseObject;
    }
    public static List<BillingEntry> ParseBillingEntries(string jsonString)
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
    
    private static List<string> GetListOfUniqueOrderDataID(List<BillingEntry> billingEntries)
    {
        try
        {
            var uniqueOrderIds = billingEntries
                .Where(entry => entry.Order != null)
                .Select(entry => entry.Order.Id)
                .Distinct()
                .ToList();
            return uniqueOrderIds;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task<JObject> GetOrderByIdAsync(string orderId)
    {
        try
        {
            var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
            var request = _allegroApiService.CreateAllegroApiRequest($"order/checkout-forms/{orderId}", accessToken);
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
            var responseObject = JObject.Parse(responseString);
            return responseObject;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

}