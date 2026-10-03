using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    [Authorize]   // use the same attribute/roles as InvoicesController
    public class PaymentsController : Controller
    {
        private readonly InvoiceService _invoiceService;
        private readonly ClientService _clientService;

        public PaymentsController(InvoiceService invoiceService, ClientService clientService)
        {
            _invoiceService = invoiceService;
            _clientService = clientService;
        }

        // Same data as the Invoices page: every invoice, with a Record Payment action on each
        public async Task<IActionResult> Index()
        {
            try
            {
                var invoices = await _invoiceService.GetInvoicesAsync();
                ViewBag.Clients = await _clientService.GetClientsAsync();   // use the same call InvoicesController.Index uses
                return View(invoices);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "The payments could not be loaded.";
                ViewBag.Clients = new List<ClientModel>();
                return View(new List<InvoiceModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(int invoiceId)
        {
            var invoice = await LoadInvoiceAsync(invoiceId);
            if (invoice == null) return NotFound();

            if (invoice.InvoiceStatus == "Paid" || invoice.InvoiceStatus == "Cancelled")
            {
                TempData["Error"] = $"Payments cannot be recorded on a {invoice.InvoiceStatus.ToLower()} invoice.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Invoice = invoice;
            return View(new RecordPaymentModel
            {
                InvoiceID = invoiceId,
                Amount = invoice.TotalAmount - invoice.AmountPaid   // pre-fill with the balance due
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecordPaymentModel model)
        {
            var invoice = await LoadInvoiceAsync(model.InvoiceID);
            if (invoice == null) return NotFound();
            ViewBag.Invoice = invoice;

            var balance = invoice.TotalAmount - invoice.AmountPaid;
            if (model.Amount > balance)
            {
                ModelState.AddModelError(nameof(model.Amount), $"The amount cannot be more than the balance due (R {balance:N2}).");
            }

            if (!ModelState.IsValid) return View(model);

            var request = new RecordPaymentRequestModel
            {
                Amount = model.Amount,
                PaymentMethod = model.PaymentMethod,
                TransactionReference = model.TransactionReference,
                Notes = model.Notes
            };

            var (success, error) = await _invoiceService.RecordPaymentAsync(model.InvoiceID, request);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "The payment could not be recorded.");
                return View(model);
            }

            TempData["Success"] = "Payment recorded.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<InvoiceModel?> LoadInvoiceAsync(int id)
        {
            try
            {
                return await _invoiceService.GetInvoiceAsync(id);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}