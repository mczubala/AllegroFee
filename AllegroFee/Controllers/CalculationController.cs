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
    public async Task<IActionResult> GetCalculatedProductFeeByIdAsync(string offerId)
    {
        try
        {
            var billingInfo = await _calculationService.GetCalculatedProductFeeByIdAsync(offerId);
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
}