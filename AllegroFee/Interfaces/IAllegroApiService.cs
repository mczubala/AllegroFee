using AllegroFee.Models;
using Newtonsoft.Json.Linq;

namespace AllegroFee.Interfaces;

public interface IAllegroApiService
{ 
    HttpRequestMessage CreateAllegroApiRequest(string relativeUrl, string accessToken);
    Task<Order> GetOrderByIdAsync(string orderId);
    Task<List<BillingEntry>> GetBillingByOfferIdAsync(string offerId);
    Task<JObject> GetAllBillingEntriesAsync();
}
