using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Security;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class ProductsController : Controller
    {

        private readonly ProductService _productService;
        private readonly ProductCategoryService _categoryService;
        //----------------------------------------------------------------------------------//
        public ProductsController(ProductService productService, ProductCategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }
        //----------------------------------------------------------------------------------//
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetProductsAsync();
            var categories = await _categoryService.GetCategoriesAsync();

            ViewBag.Categories = categories.ToDictionary(c => c.ProductCategoryID, c => c.CategoryName);

            return View(products);
        }
        //----------------------------------------------------------------------------------//
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetCategoriesAsync();

            return View(new ProductModel());
        }
        //----------------------------------------------------------------------------------//
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetCategoriesAsync();
                return View(model);
            }

            var success = await _productService.CreateProductAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Unable to create the product.");
                ViewBag.Categories = await _categoryService.GetCategoriesAsync();
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------//
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _categoryService.GetCategoriesAsync();

            return View(product);
        }
        //----------------------------------------------------------------------------------//
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetCategoriesAsync();
                return View(model);
            }

            var success = await _productService.UpdateProductAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Unable to update the product.");
                ViewBag.Categories = await _categoryService.GetCategoriesAsync();
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------//
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _productService.DeleteProductAsync(id);

            if (!success)
            {
                TempData["Error"] = "Unable to delete the product.";
            }

            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------//
        //Called via fetch() from the Create/Edit product pages - lets the user add a
        //category inline without leaving the form or building a full category page.
        //Antiforgery is intentionally skipped here since this is a same-origin AJAX
        //call behind [Authorize] already, not a public-facing endpoint.
        [HttpPost]
        public async Task<IActionResult> QuickAddCategory([FromBody] QuickAddCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CategoryName))
            {
                return BadRequest();
            }

            var category = new ProductCategoryModel
            {
                CategoryName = request.CategoryName.Trim(),
                IsActive = true
            };

            var created = await _categoryService.CreateCategoryAsync(category);

            if (created == null)
            {
                return BadRequest(new { message = "That category name may already exist." });
            }

            return Ok(created);
        }
        //----------------------------------------------------------------------------------//
        public class QuickAddCategoryRequest
        {
            public string CategoryName { get; set; } = string.Empty;
        }
    }
}
