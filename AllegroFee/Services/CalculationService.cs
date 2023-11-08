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

    public Task<JObject> GetBillingByOfferIdAsync(string offerId)
    {
        throw new NotImplementedException();
    }


    public async Task<ProductFee> GetCalculatedProductFeeByIdAsync(string offerId)
    {
        var billingEntries = await _allegroApiService.GetBillingByOfferIdAsync(offerId);
        var result = new ProductFee();
        var fixedFee = CalculateOfferFixedFee(offerId, billingEntries);
        result.Fee = fixedFee;
        return result;    
    }

    private decimal CalculateOfferFixedFee(string offerId, List<BillingEntry> billingEntries)
    {
        // Group by order id
        var groupedEntries = billingEntries
            .Where(x => x.Order != null) // exclude entries with null Order
            .GroupBy(x => x.Order.Id);

        // Calculate sum for each group
        var sumResults = groupedEntries.Select(group => new
        {
            OrderId = group.Key,
            TotalAmount = group.Sum(item => decimal.Parse(item.Value.Amount, CultureInfo.InvariantCulture))
        });
        
        // Get list of unique order ids
        var uniqueOfferIds = GetListOfUniqueOrderDataID(billingEntries);
        
        List<JObject> orders = new List<JObject>();
        foreach (var uniqueOfferId in uniqueOfferIds)
        {
            orders.Add(_allegroApiService.GetOrderByIdAsync(uniqueOfferId).Result); 
        }
        
        List<Order> ordersList = new List<Order>();
        foreach (var order in orders)
        {
            Order orderObj = JsonConvert.DeserializeObject<Order>(order.ToString());
            ordersList.Add(orderObj);
        }
        
        ProductFee productFee = new ProductFee();
        productFee.ProductId = offerId;
        var percentageFees = new List<decimal>();
        foreach (var sumResult in sumResults)
        {
            var order = ordersList.Where(x => x.Id == sumResult.OrderId).FirstOrDefault();
            var quantity = order.LineItems.Where(x => x.Offer.Id == offerId).FirstOrDefault().Quantity;
            var price = order.LineItems.Where(x => x.Offer.Id == offerId).FirstOrDefault().Price.AmountValue;
            var totalFee = sumResult.TotalAmount;
            var percentFee = totalFee / (quantity * decimal.Parse(price, CultureInfo.InvariantCulture));
            percentageFees.Add(percentFee);
        }
        
        return percentageFees.Average();;   
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
    
    

}