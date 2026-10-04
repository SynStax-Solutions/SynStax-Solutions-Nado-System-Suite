using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Security;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    //Only the Manager (Owner) can open these pages. 
    [Authorize(Roles = AppRoles.Manager)]
    public class OwnerController : Controller
    {
        private readonly JobCardService _jobCardService;
        private readonly QuoteService _quoteService;
        private readonly UserService _userService;
        private readonly ClientService _clientService;
        private readonly SupplierService _supplierService;
        private readonly EnquiryService _enquiryService;

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
            var jobCards = await _jobCardService.GetJobCardsAsync();
            var quotes = await _quoteService.GetQuotesAsync();
            var users = await _userService.GetUsersAsync();
            var clients = await _clientService.GetClientsAsync();
            var suppliers = await _supplierService.GetSuppliersAsync();
            var enquiries = await _enquiryService.GetEnquiriesAsync();

            ViewBag.JobCards = jobCards;
            ViewBag.RecentQuotes = quotes.Take(5).ToList();
            ViewBag.Users = users;
            ViewBag.Clients = clients;
            ViewBag.Suppliers = suppliers;
            ViewBag.Enquiries = enquiries;

            return View();
        }
    }
}
