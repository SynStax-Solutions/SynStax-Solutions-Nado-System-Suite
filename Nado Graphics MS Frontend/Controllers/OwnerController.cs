using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Security;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    //This controller handles pages available to the Manager (Owner).
    //Only the Manager (Owner) can open these pages.
    [Authorize(Roles = AppRoles.Manager)]
    public class OwnerController : Controller
    {
        //This stores the service used to retrieve Job Cards.
        private readonly JobCardService _jobCardService;

        //This stores the service used to retrieve Quotes.
        private readonly QuoteService _quoteService;

        //This stores the service used to retrieve Users.
        private readonly UserService _userService;

        //This stores the service used to retrieve Clients.
        private readonly ClientService _clientService;

        //This stores the service used to retrieve Suppliers.
        private readonly SupplierService _supplierService;

        //This stores the service used to retrieve Enquiries.
        private readonly EnquiryService _enquiryService;

        //This receives all required services through dependency injection.
        public OwnerController(
            JobCardService jobCardService,
            QuoteService quoteService,
            UserService userService,
            ClientService clientService,
            SupplierService supplierService,
            EnquiryService enquiryService)
        {
            _jobCardService = jobCardService;
            _quoteService = quoteService;
            _userService = userService;
            _clientService = clientService;
            _supplierService = supplierService;
            _enquiryService = enquiryService;
        }

        //This shows the Owner dashboard.
        public async Task<IActionResult> Dashboard()
        {
            //This gets all active Job Cards for the dashboard.
            var jobCards = await _jobCardService.GetJobCardsAsync();

            //This gets all Quotes for the dashboard.
            var quotes = await _quoteService.GetQuotesAsync();

            //This gets all Users for the management section.
            var users = await _userService.GetUsersAsync();

            //This gets all Clients for the management section.
            var clients = await _clientService.GetClientsAsync();

            //This gets all Suppliers for the management section.
            var suppliers = await _supplierService.GetSuppliersAsync();

            //This gets all Enquiries for the dashboard.
            var enquiries = await _enquiryService.GetEnquiriesAsync();

            //This makes the Job Cards available to the dashboard view.
            ViewBag.JobCards = jobCards;

            //This limits the Quotes shown on the dashboard to the five most recent Quotes.
            ViewBag.RecentQuotes = quotes.Take(5).ToList();

            //This makes all Users available to the dashboard view.
            ViewBag.Users = users;

            //This makes all Clients available to the dashboard view.
            ViewBag.Clients = clients;

            //This makes all Suppliers available to the dashboard view.
            ViewBag.Suppliers = suppliers;

            //This makes all Enquiries available to the dashboard view.
            ViewBag.Enquiries = enquiries;

            //This returns the Owner dashboard view.
            return View();
        }
    }
}


