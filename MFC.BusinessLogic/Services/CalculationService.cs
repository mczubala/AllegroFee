using System.Net;
using MFC.DataAccessLayer.Entities;
using MFC.DataAccessLayer.Repository;
using MFC.Interfaces;
using MFC.Models;
using MFC.Responses;

namespace MFC.Services;

public class CalculationService : ICalculationService
{
    private readonly IAllegroApiClient _allegroApiClient;
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IMfcDbRepository _mfcDbRepository;
    
    public CalculationService(IAllegroApiClient allegroApiClient, IAccessTokenProvider accessTokenProvider, IMfcDbRepository mfcDbRepository)
    {
        _allegroApiClient = allegroApiClient;
        _accessTokenProvider = accessTokenProvider;
        _mfcDbRepository = mfcDbRepository;
    }
    
    #region Public Methods 
    
    public async Task<ServiceResponse<OfferFeeDto>> GetCalculatedTotalOfferFeeByIdAsync(string offerId)
    {
        var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
        var billingResponse = await _allegroApiClient.GetBillingByOfferIdAsync(offerId,$"Bearer {accessToken}");
        
        if (billingResponse.StatusCode != HttpStatusCode.OK)
        {
            return new ServiceResponse<OfferFeeDto>(billingResponse.Error.Message, ServiceStatusCodes.StatusCode.Error);
        }

        if (billingResponse.Content == null)
        {
            return new ServiceResponse<OfferFeeDto>($"Billing entries not found for the offer id {offerId}.", ServiceStatusCodes.StatusCode.NotFound);
        }
        
        try
        {
            var result = new OfferFeeDto();
            result.OfferId = offerId;
            List<BillingEntry> billingEntries = billingResponse.Content.BillingEntries;
            var billingSum = billingEntries.Sum(billingEntry => billingEntry.Value.Amount);
            
            var totalSaleResponse = await GetCalculatedTotalOfferSaleAsync(offerId, GetListOfUniqueOrderDataId(billingEntries));
            if (totalSaleResponse.ResponseStatus != ServiceStatusCodes.StatusCode.Success)
                return new ServiceResponse<OfferFeeDto>(totalSaleResponse.Message, ServiceStatusCodes.StatusCode.Error);
            
            if (totalSaleResponse.Data == 0)
                return new ServiceResponse<OfferFeeDto>($"Total sale amount for the offer id {offerId} is 0.", ServiceStatusCodes.StatusCode.Error);
            
            result.FeePercent = Math.Abs(billingSum / totalSaleResponse.Data);


            var offerFee = new OfferFee(result.OfferId, result.FeePercent);
            _mfcDbRepository.AddOfferFee(offerFee)
                .ContinueWith(async task =>
                {
                    await _mfcDbRepository.SaveChangesAsync();
                });
            
            return new ServiceResponse<OfferFeeDto>(result);
        }
        catch (Exception e)
        {
            return new ServiceResponse<OfferFeeDto>(e.Message, ServiceStatusCodes.StatusCode.Error);
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
        
            OfferFeeDto productFee = new OfferFeeDto();
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