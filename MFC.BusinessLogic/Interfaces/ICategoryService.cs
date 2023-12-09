using MFC.Models;
using MFC.Responses;

namespace MFC.Interfaces
{
    public interface ICategoryService
    {
        Task<ServiceResponse<Category>> GetCategoryAsync(string categoryId);
    }
}