using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Security;
using Nado_Graphics_MS_Backend.Services;

namespace Nado_Graphics_MS_Backend.Controllers
{
    // CRUD API for products - the objects that appear as
    // line items on a Quote/Invoice.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }
        //--------------------------------------------------------------------------------------------------------------
        // GET api/Products - list every product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _productService.GetProductsAsync();
            return Ok(products);
        }
        //--------------------------------------------------------------------------------------------------------------
        // GET api/Products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productService.GetProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        //--------------------------------------------------------------------------------------------------------------
        // POST api/Products - creates a new product.
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            var success = await _productService.CreateProductAsync(product);

            if (!success)
            {
                return this.ApiBadRequest("The request could not be completed.");
            }

            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductID }, product);
        }
        //--------------------------------------------------------------------------------------------------------------
        // PUT api/Products/{id} - updates an existing product.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            var success = await _productService.UpdateProductAsync(id, product);

            if (!success)
            {
                return this.ApiBadRequest("The request could not be completed.");
            }

            return NoContent();
        }
        //--------------------------------------------------------------------------------------------------------------
        // DELETE api/Products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var success = await _productService.DeleteProductAsync(id);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
//-------------------------------------------------------------------END OF FILE----------------------------------------------------