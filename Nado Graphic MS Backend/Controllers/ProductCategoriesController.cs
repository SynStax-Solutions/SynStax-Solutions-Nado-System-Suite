using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Nado_Graphics_MS_Backend.Security;


namespace Nado_Graphics_MS_Backend.Controllers
{
        // Standard CRUD API for product categories (e.g. "Signage", "Print",
        // "Merchandise"). Restricted to Managers/Admins since categories are
        // structural/configuration data, not something regular staff should edit.
        [ApiController]
        [Route("api/[controller]")]
        [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly ProductCategoryService _categoryService;

        public ProductCategoriesController(ProductCategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        //--------------------------------------------------------------------------------------------------------------
        // GET api/ProductCategories - list every category.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductCategory>>> GetCategories()
        {
            var categories = await _categoryService.GetCategoriesAsync();
            return Ok(categories);
        }
        //--------------------------------------------------------------------------------------------------------------
        // GET api/ProductCategories/{id} - single category or 404.
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductCategory>> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        //--------------------------------------------------------------------------------------------------------------
        // POST api/ProductCategories - creates a new category.
        // Returns a bare BadRequest() on failure 
        [HttpPost]
        public async Task<ActionResult<ProductCategory>> CreateCategory(ProductCategory category)
        {
            var success = await _categoryService.CreateCategoryAsync(category);

            if (!success)
            {
                return BadRequest();
            }

            return CreatedAtAction(nameof(GetCategory), new { id = category.ProductCategoryID }, category);
        }
        //--------------------------------------------------------------------------------------------------------------
        // PUT api/ProductCategories/{id} - updates an existing category.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, ProductCategory category)
        {
            var success = await _categoryService.UpdateCategoryAsync(id, category);

            if (!success)
            {
                return BadRequest();
            }

            return NoContent();
        }
        //--------------------------------------------------------------------------------------------------------------
        // DELETE api/ProductCategories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
//-----------------------------------------------------------------END OF FILE-------------------------------------
