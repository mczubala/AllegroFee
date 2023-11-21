using System.Net;
using AllegroFee.Interfaces;
using AllegroFee.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace AllegroFee.Controllers;

[ApiController]
[Route("[controller]")]
public class CalculationController : ControllerBase
{
    private readonly ICalculationService _calculationService;

    public CalculationController(ICalculationService calculationService)
    {
        _calculationService = calculationService;
    }
    
    [HttpGet("get-calculated-offer-fee/{offerId}")]
    public async Task<IActionResult> GetCalculatedOfferFeeByIdAsync(string offerId)
    {
        var response = await _calculationService.GetCalculatedOfferFeeByIdAsync(offerId);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return Ok(response.Data);
        }
        else
        {
            return StatusCode((int)response.StatusCode, response.ErrorMessage);
        }
    }

    
    [HttpGet("get-calculated-total-offer-fee/{offerId}")]
    public async Task<IActionResult> GetCalculatedTotalOfferFeeByIdAsync(string offerId)
    {
        var response = await _calculationService.GetCalculatedTotalOfferFeeByIdAsync(offerId);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            return Ok(response.Data);
        }

        return StatusCode((int)response.StatusCode, response.ErrorMessage);
    }
}