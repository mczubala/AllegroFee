using System.Net;
using System.Net.Http.Headers;
using System.Text;
using AllegroFee.Interfaces;
using AllegroFee.Models;
using AllegroFee.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AllegroFee.Controllers
{
    [ApiController]
    [Route("categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        
        #region Action Methods

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetCategory(string categoryId)
        {
            try
            {
                var category = await _categoryService.GetCategoryAsync(categoryId);
                return Ok(category);
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

        [HttpGet("categories/{categoryId}")]
        public async Task<IActionResult> GetSellingConditionsForCategoryAsync(string categoryId)
        {
            try
            {
                var sellingConditions = await _categoryService.GetSellingConditionsForCategoryAsync(categoryId);
                return Ok(sellingConditions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}