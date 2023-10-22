using System.Net;
using AllegroFee.Interfaces;
using AllegroFee.Models;
using AllegroFee.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AllegroFee.Services;

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

    public async Task<Category> GetCategoryAsync(string categoryId)
    {
        var accessToken = await _accessTokenProvider.GetAccessForApplicationTokenAsync();
        var request = _allegroApiService.CreateAllegroApiRequest($"sale/categories/{categoryId}", accessToken);
        var response = await SendAllegroApiRequest(request); // Assuming you've also moved SendAllegroApiRequest to the IAllegroApiService

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ArgumentException("Category not found");
            }

            throw new BadHttpRequestException($"Failed to get category. StatusCode={response.StatusCode} Reason={response.ReasonPhrase}");
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var categoryResponse = JsonConvert.DeserializeObject<CategoryResponse>(responseString);
        var category = CreateCategoryFromResponse(categoryResponse); // If CreateCategoryFromResponse is specific to this method, it can stay here.

        return category;
    }

    public async Task<JObject> GetSellingConditionsForCategoryAsync(string categoryId)
    {
        var accessToken = await _accessTokenProvider.GetAccessForApplicationTokenAsync();
        var request = _allegroApiService.CreateAllegroApiRequest($"sale/categories/{categoryId}/selling-conditions", accessToken);
        var response = await SendAllegroApiRequest(request);

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        return JObject.Parse(responseContent);
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
