using AllegroFee.Models;
using AllegroFee.Responses;
using Newtonsoft.Json.Linq;

namespace AllegroFee.Interfaces;

public interface IAllegroApiService
{ 
    HttpRequestMessage CreateAllegroApiRequest(string relativeUrl, string accessToken);
    Task<Order> GetOrderByIdAsync(string orderId);
    Task<ServiceResponse<List<BillingEntry>>> GetBillingByOfferIdAsync(string offerId);
    Task<ServiceResponse<List<BillingEntry>>> GetAllBillingEntriesAsync();
}
