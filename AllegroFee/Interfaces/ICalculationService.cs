
using AllegroFee.Models;
using AllegroFee.Responses;

namespace AllegroFee.Interfaces
{
    public interface ICalculationService
    {
        Task<ServiceResponse<OfferFee>> GetCalculatedOfferFeeByIdAsync(string offerId);
        Task<ServiceResponse<OfferFee>> GetCalculatedTotalOfferFeeByIdAsync(string offerId);
    }
}