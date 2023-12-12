using MFC.Models;
using MFC.Responses;

namespace MFC.Interfaces
{
    public interface ICalculationService
    {
        Task<ServiceResponse<OfferFee>> GetCalculatedTotalOfferFeeByIdAsync(string offerId);
    }
}