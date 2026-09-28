using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Security;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    // Only Managers/Admins can access any action on this controller 
    // regular users/clients never see the internal Quotes management screens.
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class QuotesController: Controller
    {
        // Services injected via DI. QuoteService talks to the Quotes API,
        // ClientService/ProductService populate dropdown data used across
        // multiple actions (Create, Edit).
        private readonly QuoteService _quoteService;
        private readonly ClientService _clientService;
        private readonly ProductService _productService;
        //----------------------------------------------------------------------------------
        public QuotesController(QuoteService quoteService, ClientService clientService, ProductService productService)
        {
            _quoteService = quoteService;
            _clientService = clientService;
            _productService = productService;
        }
        //----------------------------------------------------------------------------------
        // GET /Quotes Clients are loaded into ViewBag so the view can resolve each quote's ClientID into a display name
        // without a separate API round trip per row.
        public async Task<IActionResult> Index()
        {
            var quotes = await _quoteService.GetQuotesAsync();
            ViewBag.Clients = await _clientService.GetClientsAsync();


            return View(quotes);
        }
        //----------------------------------------------------------------------------------
        // GET /Quotes/Create
        // renders the blank creation form.
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();

            //Starts the form with one blank line item so there's a row to fill in.
            var model = new QuoteModel
            {
                QuoteItems = new List<QuoteItemModel> { new QuoteItemModel() }
            };

            return View(model);
        }
        //----------------------------------------------------------------------------------
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuoteModel model)
        {
            //Empty rows can happen if a JS row was added then left blank, strip before validating.
            model.QuoteItems = model.QuoteItems.Where(i => i.ProductID > 0).ToList();

            foreach (var item in model.QuoteItems)
            {
                if (item.UnitPrice == null)
                {
                    item.UnitPrice = 0; // backend treats <= 0 as "use product's default price"
                }
            }

            // UnitPrice on QuoteItemModel is nullable (decimal?) to allow an
            // empty form input to bind successfully.
            if (!ModelState.IsValid || !model.QuoteItems.Any())
            {
                if (!model.QuoteItems.Any())
                {
                    ModelState.AddModelError("", "Add at least one product to the quote.");
                }

                await LoadDropdownsAsync();
                return View(model);
            }

            // CreateQuoteAsync returns (Success, Error) rather than a plain
            // bool so that a real failure reason from the API can be shown to the user
            var (success, error) = await _quoteService.CreateQuoteAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Unable to create the quote.");
                await LoadDropdownsAsync();
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------
        // GET /Quotes/Details/{id} - read-only view of a single quote.
        public async Task<IActionResult> Details(int id)
        {
            var quote = await _quoteService.GetQuoteAsync(id);

            if (quote == null)
            {
                return NotFound();
            }

            ViewBag.Clients = await _clientService.GetClientsAsync();

            return View(quote);
        }
        //----------------------------------------------------------------------------------
        // GET /Quotes/Edit/{id} - loads a quote for editing.
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var quote = await _quoteService.GetQuoteAsync(id);

            if (quote == null)
            {
                return NotFound();
            }

            //Editing items only makes sense while still Pending - the API also enforces this as well.
            if (quote.QuoteStatus != "Pending")
            {
                TempData["Error"] = "Only a Pending quote can be edited.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await LoadDropdownsAsync();

            return View(quote);
        }
        //----------------------------------------------------------------------------------
        // POST /Quotes/Edit - handles the edit form submission. Mirrors the
        // Create action's item cleanup and UnitPrice normalisation logic.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuoteModel model)
        {
            model.QuoteItems = model.QuoteItems.Where(i => i.ProductID > 0).ToList();

            // Same UnitPrice normalisation as Create - the Edit form uses the
            // same nullable UnitPrice field and needs the same fallback.
            foreach (var item in model.QuoteItems)
            {
                if (item.UnitPrice == null)
                {
                    item.UnitPrice = 0; // backend treats <= 0 as "use product's default price"
                }
            }

            if (!ModelState.IsValid || !model.QuoteItems.Any())
            {
                if (!model.QuoteItems.Any())
                {
                    ModelState.AddModelError("", "Add at least one product to the quote.");
                }

                await LoadDropdownsAsync();
                return View(model);
            }

            var success = await _quoteService.UpdateQuoteAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Unable to update the quote - it may no longer be editable.");
                await LoadDropdownsAsync();
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id = model.QuoteID });
        }
        //----------------------------------------------------------------------------------
        // POST /Quotes/Delete/{id} - deletes a quote and returns to the list.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _quoteService.DeleteQuoteAsync(id);

            if (!success)
            {
                TempData["Error"] = "Unable to delete the quote.";
            }

            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------
        // POST /Quotes/Accept/{id} - moves a quote to Accepted status.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var (success, error) = await _quoteService.AcceptQuoteAsync(id);

            TempData[success ? "Success" : "Error"] = success ? "Quote accepted." : error;

            return RedirectToAction(nameof(Details), new { id });
        }
        //----------------------------------------------------------------------------------
        // POST /Quotes/Reject/{id} - moves a quote to Rejected status, with a
        // required reason (e.g. from a form/model on the Details page).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var (success, error) = await _quoteService.RejectQuoteAsync(id, reason);

            TempData[success ? "Success" : "Error"] = success ? "Quote rejected." : error;

            return RedirectToAction(nameof(Details), new { id });
        }
        //----------------------------------------------------------------------------------
        // POST /Quotes/Expire/{id} - moves a quote to Expired status, same
        // pattern as Reject.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Expire(int id, string reason)
        {
            var (success, error) = await _quoteService.ExpireQuoteAsync(id, reason);

            TempData[success ? "Success" : "Error"] = success ? "Quote expired." : error;

            return RedirectToAction(nameof(Details), new { id });
        }
        //----------------------------------------------------------------------------------
        //Loads the dropdown data used by both the Create and Edit views.
        private async Task LoadDropdownsAsync()
        {
            ViewBag.Clients = await _clientService.GetClientsAsync();
            ViewBag.Products = await _productService.GetProductsAsync();
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------