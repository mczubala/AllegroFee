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
            var billingEntries = await _calculationService.GetAllBillingEntriesAsync();
            return Ok(billingEntries);
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
            return BadRequest(ex.Message);
        }
    }
}