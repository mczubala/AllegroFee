using System.Net;
using AllegroFee.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AllegroFee.Controllers;

[ApiController]
[Route("[controller]")]
public class FeeController : ControllerBase
{
    private readonly ICalculationService _calculationService;

    public FeeController(ICalculationService calculationService)
    {
        _calculationService = calculationService;
    }
    
    [HttpGet("get-offer-fee/{offerId}")]
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

    
    [HttpGet("get-total-offer-fee/{offerId}")]
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