using MFC.Models;
using Refit;

namespace MFC.Interfaces;

public interface IAllegroApiClient
{
    [Get("/order/checkout-forms/{orderId}")]
    Task<ApiResponse<Order>> GetOrderByIdAsync(string orderId, [Header("Authorization")] string authorization);

    [Get("/billing/billing-entries?offer.id={offerId}")]
    Task<ApiResponse<Billings>> GetBillingByOfferIdAsync(string offerId, [Header("Authorization")] string authorization);
        
    [Get("/sale/categories/{categoryId}")]
    Task<ApiResponse<Category>> GetCategoryByIdAsync(string categoryId, [Header("Authorization")] string authorization);
}