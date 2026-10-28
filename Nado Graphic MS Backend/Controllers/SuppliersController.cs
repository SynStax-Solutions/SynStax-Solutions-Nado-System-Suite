using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Nado_Graphics_MS_Backend.Security;

namespace Nado_Graphics_MS_Backend.Controllers
{
    //Marks the class as an API controller.
    [ApiController]

    //Defines the API route as /api/Suppliers.
    [Route("api/[controller]")]

    //Only a logged-in Manager (Owner) or PA/Admin can use these endpoints.
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class SuppliersController : ControllerBase
    {
        //The below line stores the SupplierService used to handle supplier operations.
        private readonly SupplierService _supplierService;

        //The constructor receives the SupplierService using dependency injection.
        public SuppliersController(SupplierService supplierService)
        {
            //Storing the injected SupplierService.
            _supplierService = supplierService;
        }
        //--------------------------------------------------------------------------------------------------------------//
        //Handles GET requests sent to /api/Suppliers.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetSuppliers()
        {
            //Getting all suppliers from the SupplierService.
            var suppliers = await _supplierService.GetSuppliersAsync();

            //Returning the suppliers with an HTTP 200 OK response.
            return Ok(suppliers);
        }
        //--------------------------------------------------------------------------------------------------------------//
        //Handles GET requests sent to /api/Suppliers/{id}.
        [HttpGet("{id}")]
        public async Task<ActionResult<Supplier>> GetSupplier(int id)
        {
            //Getting the selected supplier from the SupplierService.
            var supplier = await _supplierService.GetSupplierAsync(id);

            //Checking if the supplier does not exist.
            if (supplier == null)
            {
                //Returning an HTTP 404 Not Found response.
                return NotFound();
            }

            //Returning the supplier with an HTTP 200 OK response.
            return Ok(supplier);
        }
        //--------------------------------------------------------------------------------------------------------------//
        //Handles POST requests sent to /api/Suppliers.
        [HttpPost]
        public async Task<ActionResult<Supplier>> CreateSupplier(
            Supplier supplier)
        {
            //Sending the supplier to the SupplierService for creation.
            var success = await _supplierService.CreateSupplierAsync(supplier);

            //Checking if the supplier creation fails.
            if (!success)
            {
                //Returning a basic HTTP 400 Bad Request response.
                return this.ApiBadRequest("The request could not be completed.");
            }

            //Returning the supplier with an HTTP 200 OK response.
            return Ok(supplier);
        }
        //--------------------------------------------------------------------------------------------------------------//
        //Handles PUT requests sent to /api/Suppliers/{id}.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, Supplier supplier)
        {
            //Sending the supplier to the SupplierService for updating.
            var success = await _supplierService.UpdateSupplierAsync(id, supplier);

            //Checking if the supplier update fails.
            if (!success)
            {
                //Returning a basic HTTP 400 Bad Request response.
                return this.ApiBadRequest("The request could not be completed.");
            }

            //Returning an HTTP 204 No Content response.
            return NoContent();
        }
        //--------------------------------------------------------------------------------------------------------------//
        //Handles DELETE requests sent to /api/Suppliers/{id}.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            //Sending the supplier ID to the SupplierService for deletion.
            var success =
                await _supplierService.DeleteSupplierAsync(id);

            //Checking if the supplier deletion fails.
            if (!success)
            {
                //Returning an HTTP 404 Not Found response.
                return NotFound();
            }

            //Returning an HTTP 204 No Content response.
            return NoContent();
        }
    }
}
/* References
Microsoft (n.d.) Role-based authorization in ASP.NET Core. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles
 */
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//
