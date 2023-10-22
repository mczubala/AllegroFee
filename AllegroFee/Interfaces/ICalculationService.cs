using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AllegroFee.Interfaces
{
    public interface ICalculationService
    {
        Task<JObject> GetAllBillingEntriesAsync();
        Task<JObject> GetBillingByOfferIdAsync(string offerId);
        Task<JObject> GetOrderByIdAsync(string orderId);
    }
}