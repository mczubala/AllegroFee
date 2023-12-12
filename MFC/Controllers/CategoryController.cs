using System.Net;
using MFC.Interfaces;
using MFC.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MFC.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        
        #region Action Methods

        [HttpGet("{categoryId:int}")]
        public async Task<IActionResult> GetCategoryById(string categoryId)
        {
            var response = await _categoryService.GetCategoryAsync(categoryId);
            if (response.ResponseStatus == ServiceStatusCodes.StatusCode.Success)
            {
                return Ok(response.Data);
            }

            return StatusCode((int)HttpStatusCode.BadRequest, response.Message);        }
        
        #endregion
    }
}