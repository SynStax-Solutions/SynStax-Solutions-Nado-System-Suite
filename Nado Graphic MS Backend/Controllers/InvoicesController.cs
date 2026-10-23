using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
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
    public class InvoicesController : ControllerBase
    {
        // The constructor initializes the InvoicesController with an instance of InvoiceService, enabling it to perform operations on invoices.
        private readonly InvoiceService _invoiceService;
        private readonly InvoicePDFService _invoicePdfService;

        public InvoicesController(InvoiceService invoiceService, InvoicePDFService invoicePDFService)
        {
            _invoiceService = invoiceService;
            _invoicePdfService = invoicePDFService;
        }
        //--------------------------------------------------------------------------------------------------------------
        
        // The GetInvoices method retrieves a list of all invoices from the database and returns them in the response.
        // The GetInvoice method retrieves a specific invoice by its ID and returns it in the response. If the invoice is not found,
        // it returns a 404 Not Found response.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
        {
            var invoices = await _invoiceService.GetInvoicesAsync();
            return Ok(invoices);
        }

        //-------------------------------------------------------------------------
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var salesRepName = User.FindFirst(AppClaims.Name)?.Value ?? "Nado Graphics";
            var pdf = await _invoicePdfService.GenerateInvoicePdfAsync(id, salesRepName);

            if (pdf == null)
            {
                return NotFound();
            }

            return File(pdf, "application/pdf", $"Invoice-{id}.pdf");
        }
        //--------------------------------------------------------------------------------------------------------------
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
        //--------------------------------------------------------------------------------------------------------------
        // The CreateInvoiceManually method creates a new invoice based on the provided invoice data and returns the created invoice in the response.
        // If the creation fails, it returns a 400 Bad Request response with an error message.

        [HttpPost("manually")]
        public async Task<ActionResult<Invoice>> CreateInvoiceManually(CreateManualInvoiceRequest request)
        {
            var (success, error, invoice) = await _invoiceService.CreateInvoiceManuallyAsync(request);

            if (!success)
            {
                return this.ApiBadRequest(error);
            }

            return CreatedAtAction(nameof(GetInvoice), new { id = invoice!.InvoiceID }, invoice);
        }
        //--------------------------------------------------------------------------------------------------------------//

        // The CreateInvoiceFromQuote method creates a new invoice based on an existing quote identified by its ID and returns the created invoice in the response.
        // If the creation fails, it returns a 400 Bad Request response with an error message.
        [HttpPost("from-quote/{quoteId}")]
        public async Task<ActionResult<Invoice>> CreateInvoiceFromQuote(int quoteId)
        {
            var (success, error, invoice) = await _invoiceService.CreateInvoiceFromQuoteAsync(quoteId);

            if (!success)
            {
                return this.ApiBadRequest(error);
            }

            return CreatedAtAction(nameof(GetInvoice), new { id = invoice!.InvoiceID }, invoice);
        }
        //--------------------------------------------------------------------------------------------------------------//
        // The UpdateInvoice method updates an existing invoice identified by its ID with the provided invoice data and returns a 204 No Content response if successful.
        // If the update fails, it returns a 400 Bad Request response.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, Invoice invoice)
        {
            var success = await _invoiceService.UpdateInvoiceAsync(id, invoice);

            if (!success)
            {
                return this.ApiBadRequest("The request could not be completed.");
            }

            return NoContent();
        }
        //--------------------------------------------------------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var success = await _invoiceService.DeleteInvoiceAsync(id);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        //--------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------
        // The RecordPayment method records a payment for an invoice identified by its ID using the provided payment request data.
        // If the payment recording is successful, it returns a 204 No Content response; otherwise, it returns a 400 Bad Request response with an error message.
        [HttpPost("{id}/payments")]
        public async Task<IActionResult> RecordPayment(int id, RecordPaymentRequest request)
        {
            var (success, error) = await _invoiceService.RecordPaymentAsync(id, request);

            if (!success)
            {
                return this.ApiBadRequest(error);
            }

            return NoContent();
        }
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Handles the HTTP PUT request to extend an overdue invoice by its ID. It accepts an ExtensionRequestedState object containing the reason for the extension.  
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id}/extension")]
        public async Task<IActionResult> ExtendInvoice(int id, ExtendInvoiceRequest request)
        {
            var (success, error) = await _invoiceService.ExtensionInvoiceAsync(id, request.Reason);

            if (!success)
            {
                return this.ApiBadRequest(error);
            }

            return NoContent();
        }
        //--------------------------------------------------------------------------------------------------------------
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelInvoice(int id, ExtendInvoiceRequest request)
        {
            var (success, error) = await _invoiceService.CancelInvoiceAsync(id, request.Reason);

            if (!success)
            {
                return this.ApiBadRequest(error);
            }

            return NoContent();
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------//