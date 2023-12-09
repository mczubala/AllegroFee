using System.Net;
using MFC.Interfaces;
using MFC.Models;
using MFC.Responses;
using Newtonsoft.Json;

namespace MFC.Services;

public class CategoryService : ICategoryService
{
    private readonly IAllegroApiService _allegroApiService;
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IHttpClientFactory _clientFactory;

    public CategoryService(IAllegroApiService allegroApiService, IAccessTokenProvider accessTokenProvider, IHttpClientFactory clientFactory)
    {
        _allegroApiService = allegroApiService;
        _accessTokenProvider = accessTokenProvider;
        _clientFactory = clientFactory;
    }

    public async Task<ServiceResponse<Category>> GetCategoryAsync(string categoryId)
    {
        try
        {
            var accessToken = await _accessTokenProvider.GetAccessForApplicationTokenAsync();
            var request = _allegroApiService.CreateAllegroApiRequest($"sale/categories/{categoryId}", accessToken);
            var response = await SendAllegroApiRequest(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new ServiceResponse<Category>(null, $"Category not found", ServiceStatusCodes.StatusCode.NotFound);
                }

                return new ServiceResponse<Category>(null, $"Failed to get category. StatusCode={response.StatusCode} Reason={response.ReasonPhrase}", ServiceStatusCodes.StatusCode.Error);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var categoryResponse = JsonConvert.DeserializeObject<CategoryResponse>(responseString);
            var category = CreateCategoryFromResponse(categoryResponse);

            return new ServiceResponse<Category>(category);
        }
        catch (Exception ex)
        {
            // Log the exception if needed
            return new ServiceResponse<Category>(null, $"Internal server error: {ex.Message}", ServiceStatusCodes.StatusCode.Error);
        }
    }

    #region Private Methods
    private Category CreateCategoryFromResponse(CategoryResponse response)
    {
        return new Category
        {
            Id = response.Id,
            Name = response.Name, };
    }

    private async Task<HttpResponseMessage> SendAllegroApiRequest(HttpRequestMessage request)
    {
        var client = _clientFactory.CreateClient();
        return await client.SendAsync(request);
    }

    private T DeserializeJson<T>(string jsonString)
    {
        return JsonConvert.DeserializeObject<T>(jsonString);
    }
        
    #endregion
}
