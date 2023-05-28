using System.Net;
using AllegroFee.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace AllegroFee.Controllers;

[ApiController]
[Route("[controller]")]
public class CalculationController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly string AllegroApiBaseUrl;
    private readonly CalculationService _calculationService;

    public CalculationController(IHttpClientFactory clientFactory, IAccessTokenProvider accessTokenProvider,
        IConfiguration configuration, CalculationService calculationService)
    {
        _clientFactory = clientFactory;
        _accessTokenProvider = accessTokenProvider;
        AllegroApiBaseUrl = configuration.GetValue<string>("AllegroApiBaseUrl");
        _calculationService = calculationService;
    }

    [HttpGet("calculation/offerfee/{offerId}")]
    public async Task<IActionResult> GetBillingByOfferIdAsync(string offerId)
    {
        try
        {
            var billingInfo = await _calculationService.GetBillingByOfferIdAsync(offerId);
            return Ok(billingInfo);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message); // Internal Server Error
        }

    }
    
    [HttpGet("billing-entries")]
    public async Task<IActionResult> GetAllBillingEntriesAsync()
    {
        try
        {
            var accessToken = await _accessTokenProvider.GetAccessForUserTokenAsync();
            var request = CreateAllegroApiRequest("billing/billing-entries", accessToken);

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                return BadRequest($"Failed to get billing information. StatusCode={response.StatusCode} Reason={response.ReasonPhrase}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JObject.Parse(responseString);
            return Ok(responseObject);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    private HttpRequestMessage CreateAllegroApiRequest(string relativeUrl, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{AllegroApiBaseUrl}/{relativeUrl}");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        return request;
    }

}