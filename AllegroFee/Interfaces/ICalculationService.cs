
using AllegroFee.Models;

namespace AllegroFee.Interfaces
{
    public interface ICalculationService
    {
        Task<ProductFee> GetCalculatedProductFeeByIdAsync(string offerId);
    }
}