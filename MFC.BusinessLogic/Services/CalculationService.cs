using System.Globalization;
using System.Net;
using MFC.Interfaces;
using MFC.Models;
using MFC.Responses;

namespace MFC.Services;

public class CalculationService : ICalculationService
{
    private readonly IAllegroApiClient _allegroApiClient;
    private readonly IAccessTokenProvider _accessTokenProvider;

    public CalculationService(IAllegroApiClient allegroApiClient, IAccessTokenProvider accessTokenProvider)
    {
        _allegroApiClient = allegroApiClient;
        _accessTokenProvider = accessTokenProvider;
    }
    
    #region Public Methods 
    
    public async Task<ServiceResponse<OfferFee>> GetCalculatedTotalOfferFeeByIdAsync(string offerId)
    {
        var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
        var billingResponse = await _allegroApiClient.GetBillingByOfferIdAsync(offerId,$"Bearer {accessToken}");
        
        if (billingResponse.StatusCode != HttpStatusCode.OK)
        {
            return new ServiceResponse<OfferFee>(billingResponse.Error.Message, ServiceStatusCodes.StatusCode.Error);
        }

        if (billingResponse.Content == null)
        {
            return new ServiceResponse<OfferFee>($"Billing entries not found for the offer id {offerId}.", ServiceStatusCodes.StatusCode.NotFound);
        }
        
        try
        {
            var result = new OfferFee();
            result.OfferId = offerId;
            List<BillingEntry> billingEntries = billingResponse.Content.BillingEntries;
            var billingSum = billingEntries.Sum(billingEntry => billingEntry.Value.Amount);
            
            var totalSaleResponse = await GetCalculatedTotalOfferSaleAsync(offerId, GetListOfUniqueOrderDataId(billingEntries));
            if (totalSaleResponse.ResponseStatus != ServiceStatusCodes.StatusCode.Success)
                return new ServiceResponse<OfferFee>(totalSaleResponse.Message, ServiceStatusCodes.StatusCode.Error);
            
            if (totalSaleResponse.Data == 0)
                return new ServiceResponse<OfferFee>($"Total sale amount for the offer id {offerId} is 0.", ServiceStatusCodes.StatusCode.Error);
            
            result.Fee = Math.Abs(billingSum / totalSaleResponse.Data);
            return new ServiceResponse<OfferFee>(result);
        }
        catch (Exception e)
        {
            return new ServiceResponse<OfferFee>(e.Message, ServiceStatusCodes.StatusCode.Error);
        }
    }

    #endregion
    
    #region Private Methods   

    private decimal CalculateOfferFixedFee(string offerId, List<BillingEntry> billingEntries)
    {
        decimal result = 0;
        try
        {
            // Group by order id
            var groupedEntries = billingEntries
                .Where(x => x.Order != null) // exclude entries with null Order
                .GroupBy(x => x.Order.Id);
    
            // Calculate sum for each group
            var sumResults = groupedEntries.Select(group => new
            {
                OrderId = group.Key,
                //TotalAmount = group.Sum(item => decimal.Parse(item.Value.Amount, CultureInfo.InvariantCulture))
                TotalAmount = group.Sum(item => item.Value.Amount)
            });
        
            // Get list of unique order ids
            var uniqueOrderIds = GetListOfUniqueOrderDataId(billingEntries);
        
            List<Order> orders = new List<Order>();
            var accessToken = _accessTokenProvider.GetAccessForUserTokenAsync();
            foreach (var uniqueOrderId in uniqueOrderIds)
            {
                var order = _allegroApiClient.GetOrderByIdAsync(uniqueOrderId, $"Bearer {accessToken}");
                orders.Add(order.Result.Content);
            }
        
            OfferFee productFee = new OfferFee();
            productFee.OfferId = offerId;
            var percentageFees = new List<decimal>();
            foreach (var sumResult in sumResults)
            {
                var order = orders.Where(x => x.Id == sumResult.OrderId).FirstOrDefault();
                var quantity = order.LineItems.Where(x => x.Offer.Id == offerId).FirstOrDefault().Quantity;
                var price = order.LineItems.Where(x => x.Offer.Id == offerId).FirstOrDefault().Price.Amount;
                var totalFee = sumResult.TotalAmount;
                var percentFee = totalFee / (quantity * price);
    
                percentageFees.Add(percentFee);
                result = percentageFees.Average();;   
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    
        return result;
    
    }
    private async Task<ServiceResponse<decimal>> GetCalculatedTotalOfferSaleAsync(string offerId, List<string> orderIds)
    {
        try
        {
            var orders = new List<Order>();
            foreach (var orderId in orderIds)
            {
                //var order = await _allegroApiService.GetOrderByIdAsync(orderId);
                var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
                var order = await _allegroApiClient.GetOrderByIdAsync(orderId, $"Bearer {accessToken}");
                
                if (order.StatusCode == HttpStatusCode.NotFound)
                {
                    return new ServiceResponse<decimal>($"Order with ID {orderId} not found.", ServiceStatusCodes.StatusCode.NotFound);
                }
                orders.Add(order.Content);
            }

            // Calculate total sale amount for the specific offerId
            decimal result = orders.SelectMany(order => order.LineItems)
                .Where(item => item.Offer.Id == offerId)
                .Sum(item => item.Price.Amount * item.Quantity);

            return new ServiceResponse<decimal>(result);
        }
        catch (Exception ex)
        {
            return new ServiceResponse<decimal>($"Internal server error: {ex.Message}", ServiceStatusCodes.StatusCode.Error); 
        }
    }
    private static List<string> GetListOfUniqueOrderDataId(List<BillingEntry> billingEntries)
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
    
    #endregion
}