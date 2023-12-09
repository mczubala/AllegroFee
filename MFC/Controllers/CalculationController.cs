using System.Net;
using MFC.Interfaces;
using MFC.Responses;

using Microsoft.AspNetCore.Mvc;

namespace MFC.Controllers;

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

        if (response.ResponseStatus == ServiceStatusCodes.StatusCode.Success)
        {
            return Ok(response.Data);
        }
        return StatusCode((int)HttpStatusCode.BadRequest, response.Message);
    }
    
    [HttpGet("get-total-offer-fee/{offerId}")]
    public async Task<IActionResult> GetCalculatedTotalOfferFeeByIdAsync(string offerId)
    {
        var response = await _calculationService.GetCalculatedTotalOfferFeeByIdAsync(offerId);
        if (response.ResponseStatus == ServiceStatusCodes.StatusCode.Success)
        {
            return Ok(response.Data);
        }

        return StatusCode((int)HttpStatusCode.BadRequest, response.Message);
    }
}