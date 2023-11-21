using System.Globalization;
using System.Net;
using AllegroFee.Interfaces;
using AllegroFee.Models;
using AllegroFee.Responses;


namespace AllegroFee.Services;

public class CalculationService : ICalculationService
{
    private readonly IAllegroApiService _allegroApiService;

    public CalculationService(IAllegroApiService allegroApiService)
    {
        _allegroApiService = allegroApiService;
    }

    #region Public  
    
    public async Task<ServiceResponse<OfferFee>> GetCalculatedOfferFeeByIdAsync(string offerId)
    {
        var billingResponse = await _allegroApiService.GetBillingByOfferIdAsync(offerId);
        if (billingResponse.StatusCode != HttpStatusCode.OK)
        {
            return new ServiceResponse<OfferFee>(billingResponse.ErrorMessage, billingResponse.StatusCode);
        }

        if (billingResponse.Data == null || !billingResponse.Data.Any())
        {
            return new ServiceResponse<OfferFee>($"Billing entries not found for the offer id {offerId}.", HttpStatusCode.NotFound);
        }
        try
        {
            var result = new OfferFee();
            var fixedFee = CalculateOfferFixedFee(offerId, billingResponse.Data);
            result.Fee = fixedFee;
            return new ServiceResponse<OfferFee>(result);
        }
        catch (Exception ex)
        {
            return new ServiceResponse<OfferFee>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }


    public async Task<ServiceResponse<OfferFee>> GetCalculatedTotalOfferFeeByIdAsync(string offerId)
    {
        var billingResponse = await _allegroApiService.GetBillingByOfferIdAsync(offerId);
        if (billingResponse.StatusCode != HttpStatusCode.OK)
        {
            return new ServiceResponse<OfferFee>(billingResponse.ErrorMessage, billingResponse.StatusCode);
        }

        if (billingResponse.Data == null || !billingResponse.Data.Any())
        {
            return new ServiceResponse<OfferFee>($"Billing entries not found for the offer id {offerId}.", HttpStatusCode.NotFound);
        }
        
        try
        {
            var result = new OfferFee();
            result.OfferId = offerId;
            var billingEntries = billingResponse.Data;
            var billingSum = billingEntries.Sum(x => decimal.Parse(x.Value.Amount, CultureInfo.InvariantCulture));
        
            var totalSaleResponse = await GetCalculatedTotalOfferSaleAsync(offerId, GetListOfUniqueOrderDataID(billingEntries));
            if (totalSaleResponse.StatusCode != HttpStatusCode.OK)
                return new ServiceResponse<OfferFee>(totalSaleResponse.ErrorMessage, totalSaleResponse.StatusCode);

            result.Fee = billingSum / totalSaleResponse.Data;
            return new ServiceResponse<OfferFee>(result);
        }
        catch (Exception e)
        {
            return new ServiceResponse<OfferFee>(e.Message, HttpStatusCode.InternalServerError);
        }
    }
    public async Task<ServiceResponse<decimal>> GetCalculatedTotalOfferSaleAsync(string offerId, List<string> orderIds)
    {
        try
        {
            var orders = new List<Order>();
            foreach (var orderId in orderIds)
            {
                var order = await _allegroApiService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return new ServiceResponse<decimal>($"Order with ID {orderId} not found.", HttpStatusCode.NotFound);
                }
                orders.Add(order);
            }

            // Calculate total sale amount for the specific offerId
            decimal result = orders.SelectMany(order => order.LineItems)
                .Where(item => item.Offer.Id == offerId)
                .Sum(item => decimal.Parse(item.Price.AmountValue, CultureInfo.InvariantCulture) * item.Quantity);

            return new ServiceResponse<decimal>(result);
        }
        catch (Exception ex)
        {
            return new ServiceResponse<decimal>($"Internal server error: {ex.Message}", HttpStatusCode.InternalServerError); 
        }
    }

    #endregion
    

    
    #region Private     

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
                TotalAmount = group.Sum(item => decimal.Parse(item.Value.Amount, CultureInfo.InvariantCulture))
            });
        
            // Get list of unique order ids
            var uniqueOrderIds = GetListOfUniqueOrderDataID(billingEntries);
        
            List<Order> orders = new List<Order>();
            foreach (var uniqueOrderId in uniqueOrderIds)
            {
                orders.Add(_allegroApiService.GetOrderByIdAsync(uniqueOrderId).Result); 
            }
        
            OfferFee productFee = new OfferFee();
            productFee.OfferId = offerId;
            var percentageFees = new List<decimal>();
            foreach (var sumResult in sumResults)
            {
                var order = orders.Where(x => x.Id == sumResult.OrderId).FirstOrDefault();
                var quantity = order.LineItems.Where(x => x.Offer.Id == offerId).FirstOrDefault().Quantity;
                var price = order.LineItems.Where(x => x.Offer.Id == offerId).FirstOrDefault().Price.AmountValue;
                var totalFee = sumResult.TotalAmount;
                var percentFee = totalFee / (quantity * decimal.Parse(price, CultureInfo.InvariantCulture));
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

    #endregion
    
    }

}