using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly SupplierService _supplierService;
        //----------------------------------------------------------------------------------//
        public SuppliersController(SupplierService supplierService)
        {
            _supplierService = supplierService;
        }
        //----------------------------------------------------------------------------------//
        public async Task<IActionResult> Index()
        {
            var suppliers =
                await _supplierService.GetSuppliersAsync();

            return View(suppliers);
        }
        //----------------------------------------------------------------------------------//

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var suppliers = await _supplierService.GetSuppliersAsync();

            ViewBag.Suppliers = suppliers;

            return View();
        }
        //----------------------------------------------------------------------------------//

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Suppliers = await _supplierService.GetSuppliersAsync();

                return View(model);
            }

            var success = await _supplierService.CreateSupplierAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Unable to create supplier, a supplier with similar information already exists.");

                ViewBag.Suppliers = await _supplierService.GetSuppliersAsync();

                return View(model);
            }

            return RedirectToAction(nameof(Create));
        }
        //----------------------------------------------------------------------------------//

        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _supplierService.GetSupplierAsync(id);

            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }
        //----------------------------------------------------------------------------------//

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _supplierService.GetSupplierAsync(id);

            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }
        //----------------------------------------------------------------------------------//

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            SupplierModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _supplierService.UpdateSupplierAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Unable to update the supplier.");

                return View(model);
            }

            return RedirectToAction(nameof(Details));
        }
        //----------------------------------------------------------------------------------//
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _supplierService.DeleteSupplierAsync(id);

            if (!success)
            {
                TempData["Error"] = "Unable to delete the supplier.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//