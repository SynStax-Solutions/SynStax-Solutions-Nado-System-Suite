using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Models.InvoiceStates;
using Nado_Graphics_MS_Backend.Security;
using Nado_Graphics_MS_Backend.Services;

namespace Nado_Graphics_MS_Backend.Controllers
{
    // The InvoicesController class handles HTTP requests related to invoices, providing endpoints for creating, retrieving,
    // updating, deleting, recording payments, and extending invoices. It uses the InvoiceService to perform business logic
    // and is secured with role-based authorization, allowing only managers or administrators to access its endpoints.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class InvoicesController : Controller
    {
        // The InvoiceService is injected into the controller to handle business logic related to invoices.
        private readonly InvoiceService _invoiceService;
        // The constructor initializes the InvoicesController with an instance of InvoiceService, enabling it to perform operations on invoices.
        public InvoicesController(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        //----------------------------------------------------
        // The GetInvoices method retrieves a list of all invoices from the database and returns them in the response.
        // The GetInvoice method retrieves a specific invoice by its ID and returns it in the response. If the invoice is not found,
        // it returns a 404 Not Found response.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
        {
            var invoices = await _invoiceService.GetInvoicesAsync();
            return Ok(invoices);
        }

        // The GetInvoice method retrieves a specific invoice by its ID and returns it in the response. If the invoice is not found,
        // it returns a 404 Not Found response.
        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(int id)
        {
            var invoice = await _invoiceService.GetInvoiceAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            return Ok(invoice);
        }

        //----------------------------------------------------
        // The CreateInvoiceManually method creates a new invoice based on the provided invoice data and returns the created invoice in the response.
        // If the creation fails, it returns a 400 Bad Request response with an error message.
        [HttpPost("manually")]
        public async Task<ActionResult<Invoice>> CreateInvoiceManually(Invoice invoice)
        {
            var (success, error) = await _invoiceService.CreateInvoiceManuallyAsync(invoice);

            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.InvoiceID }, invoice);
        }

        // The CreateInvoiceFromQuote method creates a new invoice based on an existing quote identified by its ID and returns the created invoice in the response.
        // If the creation fails, it returns a 400 Bad Request response with an error message.
        [HttpPost("from-quote/{quoteId}")]
        public async Task<ActionResult<Invoice>> CreateInvoiceFromQuote(int quoteId)
        {
            var (success, error, invoice) = await _invoiceService.CreateInvoiceFromQuoteAsync(quoteId);
            if (!success) return BadRequest(new { message = error });
            return CreatedAtAction(nameof(GetInvoice), new { id = invoice!.InvoiceID }, invoice);
        }

        // The UpdateInvoice method updates an existing invoice identified by its ID with the provided invoice data and returns a 204 No Content response if successful.
        // If the update fails, it returns a 400 Bad Request response.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, Invoice invoice)
        {
            var success = await _invoiceService.UpdateInvoiceAsync(id, invoice);
            if (!success)
            {
                return BadRequest();
            }

            return NoContent();
        }

        // The DeleteInvoiceAsync method deletes an existing invoice identified by its ID and returns a 204 No Content response if successful.
        // If the invoice is not found, it returns a 404 Not Found response.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceAsync(int id)
        {
            var success = await _invoiceService.DeleteInvoiceAsync(id);
            if (!success)
            {
                return NotFound();
            }

            // 204 No Content
            // meaningful to return in the body.
            return NoContent();
        }

        //----------------------------------------------------
        // The RecordPayment method records a payment for an invoice identified by its ID using the provided payment request data.
        // If the payment recording is successful, it returns a 204 No Content response; otherwise, it returns a 400 Bad Request response with an error message.
        [HttpPost("{id}/payments")]
        public async Task<IActionResult> RecordPayment(int id, RecordPaymentRequest request)
        {
            var (success, error) = await _invoiceService.RecordPaymentAsync(id, request);
            if (!success) return BadRequest(new { message = error });
            return NoContent();
        }

        /// <summary>
        /// Handles the HTTP PUT request to extend an overdue invoice by its ID. It accepts an ExtensionRequestedState object containing the reason for the extension.  
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id}/extension")]
        public async Task<IActionResult> ExtendInvoice(int id, ExtensionRequestedState request)
        {
            var (success, error) = await _invoiceService.ExtensionInvoiceAsync(id, request.Reason);
            if (!success) return BadRequest(new { message = error });
            return NoContent();
        }
    }
}
