using System.Threading.Tasks;
using AllegroFee.Models;
using Newtonsoft.Json.Linq;

namespace AllegroFee.Interfaces
{
    public interface ICategoryService
    {
        Task<Category> GetCategoryAsync(string categoryId);
        Task<JObject> GetSellingConditionsForCategoryAsync(string categoryId);
    }
}