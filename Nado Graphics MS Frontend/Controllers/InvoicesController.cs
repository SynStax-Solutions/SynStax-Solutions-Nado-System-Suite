using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Nado_Graphics_MS_Frontend.Security;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class InvoicesController : Controller
    {
        private readonly InvoiceService _invoiceService;
        private readonly ClientService _clientService;
        private readonly ProductService _productService;
        //----------------------------------------------------------------------------------
        public InvoicesController(InvoiceService invoiceService, ClientService clientService, ProductService productService)
        {
            _invoiceService = invoiceService;
            _clientService = clientService;
            _productService = productService;
        }
        //----------------------------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var invoices = await _invoiceService.GetInvoicesAsync();
            ViewBag.Clients = await _clientService.GetClientsAsync();

            return View(invoices);
        }
        //----------------------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();

            var model = new InvoiceModel
            {
                InvoiceItems = new List<InvoiceItemModel> { new InvoiceItemModel() }
            };

            return View(model);
        }
        //----------------------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceModel model)
        {

            foreach (var key in ModelState.Keys.Where(k => k.EndsWith(".UnitPrice")).ToList())
            {
                ModelState.Remove(key);
            }

            model.InvoiceItems = model.InvoiceItems.Where(i => i.ProductID > 0).ToList();

            if (!ModelState.IsValid || !model.InvoiceItems.Any())
            {


                if (!model.InvoiceItems.Any())
                {
                    ModelState.AddModelError("", "Add at least one product to the invoice.");
                }

                await LoadDropdownsAsync();
                return View(model);
            }

            var success = await _invoiceService.CreateInvoiceManuallyAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Unable to create the invoice.");
                await LoadDropdownsAsync();
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------
        //Called from a quote's Details page when it's Accepted.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromQuote(int quoteId)
        {
            var (success, error) = await _invoiceService.CreateInvoiceFromQuoteAsync(quoteId);

            TempData[success ? "Success" : "Error"] = success ? "Invoice created from quote." : error;

            return RedirectToAction("Details", "Quotes", new { id = quoteId });
        }
        //----------------------------------------------------------------------------------
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _invoiceService.GetInvoiceAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            ViewBag.Clients = await _clientService.GetClientsAsync();
            ViewBag.Products = await _productService.GetProductsAsync();

            return View(invoice);
        }
        //----------------------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var invoice = await _invoiceService.GetInvoiceAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            if (invoice.InvoiceStatus == "Paid" || invoice.InvoiceStatus == "Cancelled")
            {
                TempData["Error"] = "A paid or cancelled invoice can't be edited.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await LoadDropdownsAsync();

            return View(invoice);
        }
        //----------------------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InvoiceModel model)
        {
            model.InvoiceItems = model.InvoiceItems.Where(i => i.ProductID > 0).ToList();

            if (!ModelState.IsValid || !model.InvoiceItems.Any())
            {
                if (!model.InvoiceItems.Any())
                {
                    ModelState.AddModelError("", "Add at least one product to the invoice.");
                }

                await LoadDropdownsAsync();
                return View(model);
            }

            var success = await _invoiceService.UpdateInvoiceAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Unable to update the invoice - it may no longer be editable, or the new total is less than what's already been paid.");
                await LoadDropdownsAsync();
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id = model.InvoiceID });
        }
        //----------------------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _invoiceService.DeleteInvoiceAsync(id);

            if (!success)
            {
                TempData["Error"] = "Unable to delete the invoice.";
            }

            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------//
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(int id, decimal amount, string paymentMethod, string? transactionReference, string? notes)
        {
            var request = new RecordPaymentRequestModel
            {
                Amount = amount,
                PaymentMethod = paymentMethod,
                TransactionReference = transactionReference,
                Notes = notes
            };

            var (success, error) = await _invoiceService.RecordPaymentAsync(id, request);

            TempData[success ? "Success" : "Error"] = success ? "Payment recorded." : error;

            return RedirectToAction(nameof(Details), new { id });
        }
        //----------------------------------------------------------------------------------//
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Extend(int id, string reason)
        {
            var (success, error) = await _invoiceService.ExtendInvoiceAsync(id, reason);

            TempData[success ? "Success" : "Error"] = success ? "Invoice extended." : error;

            return RedirectToAction(nameof(Details), new { id });
        }
        //----------------------------------------------------------------------------------//
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string reason)
        {
            var (success, error) = await _invoiceService.CancelInvoiceAsync(id, reason);

            TempData[success ? "Success" : "Error"] = success ? "Invoice cancelled." : error;

            return RedirectToAction(nameof(Details), new { id });
        }
        //----------------------------------------------------------------------------------//
        private async Task LoadDropdownsAsync()
        {
            ViewBag.Clients = await _clientService.GetClientsAsync();
            ViewBag.Products = await _productService.GetProductsAsync();
        }

        public async Task<IActionResult> DownloadPdf(int id)
        {
            var pdf = await _invoiceService.DownloadInvoicePdfAsync(id);

            if (pdf == null)
            {
                TempData["Error"] = "The invoice PDF could not be generated.";
                return RedirectToAction("Index");
            }

            return File(pdf, "application/pdf", $"Invoice-{id}.pdf");
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------//