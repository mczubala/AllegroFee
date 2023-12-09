using MFC.Models;
using MFC.Responses;

namespace MFC.Interfaces;

public interface IAllegroApiService
{ 
    HttpRequestMessage CreateAllegroApiRequest(string relativeUrl, string accessToken);
    Task<Order> GetOrderByIdAsync(string orderId);
    Task<ExternalApiResponse<List<BillingEntry>>> GetBillingByOfferIdAsync(string offerId);
    Task<ExternalApiResponse<List<BillingEntry>>> GetAllBillingEntriesAsync();
}
