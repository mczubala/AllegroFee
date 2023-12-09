using System.Net;
using MFC.Interfaces;
using MFC.Responses;
using MFC.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace MFC.Controllers;

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

        if (response.ResponseStatus == ServiceStatusCodes.StatusCode.Success)
        {
            return Ok(response.Data);
        }
        return StatusCode((int)HttpStatusCode.BadRequest, response.Message);
    }
    
    [HttpGet("get-calculated-total-offer-fee/{offerId}")]
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