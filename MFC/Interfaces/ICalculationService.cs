
using MFC.Models;
using MFC.Responses;

namespace MFC.Interfaces
{
    public interface ICalculationService
    {
        Task<ServiceResponse<OfferFee>> GetCalculatedOfferFeeByIdAsync(string offerId);
        Task<ServiceResponse<OfferFee>> GetCalculatedTotalOfferFeeByIdAsync(string offerId);
    }
}