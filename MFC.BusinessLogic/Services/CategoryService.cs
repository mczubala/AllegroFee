using System.Net;
using MFC.Interfaces;
using MFC.Models;
using MFC.Responses;
using Newtonsoft.Json;

namespace MFC.Services;

public class CategoryService : ICategoryService
{
    private readonly IAllegroApiClient _allegroApiClient;
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IHttpClientFactory _clientFactory;
private readonly IAllegroApiService _allegroApiService;
    

    public CategoryService(IAccessTokenProvider accessTokenProvider, IHttpClientFactory clientFactory, IAllegroApiClient allegroApiClient, IAllegroApiService allegroApiService)
    {
        _accessTokenProvider = accessTokenProvider;
        _clientFactory = clientFactory;
        _allegroApiClient = allegroApiClient;
        _allegroApiService = allegroApiService;
    }

    public async Task<ServiceResponse<Category>> GetCategoryAsync(string categoryId)
    {
        try
        {
            var accessToken = await _accessTokenProvider.GetAccessForApplicationTokenAsync();
            var categoryResponse = await _allegroApiClient.GetCategoryByIdAsync(categoryId, $"Bearer {accessToken}");
            
            if (!categoryResponse.IsSuccessStatusCode)
            {
                if (categoryResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return new ServiceResponse<Category>(null, $"Category not found", ServiceStatusCodes.StatusCode.NotFound);
                }

                return new ServiceResponse<Category>(null, $"Failed to get category. StatusCode={categoryResponse.StatusCode} Reason={categoryResponse.ReasonPhrase}", ServiceStatusCodes.StatusCode.Error);
            }


            var category = categoryResponse.Content;

            return new ServiceResponse<Category>(category);
        }
        catch (Exception ex)
        {
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
