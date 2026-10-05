using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Nado_Graphics_MS_Frontend.Security;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class StatementsController : Controller
    {
        private readonly StatementService _statementService;
        private readonly ClientService _clientService;
        private readonly OrganisationService _organisationService;
        //----------------------------------------------------------------------------------
        public StatementsController(StatementService statementService, ClientService clientService, OrganisationService organisationService)
        {
            _statementService = statementService;
            _clientService = clientService;
            _organisationService = organisationService;
        }
        //----------------------------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var statements = await _statementService.GetStatementsAsync();

            ViewBag.Clients = await _clientService.GetClientsAsync();
            ViewBag.Organisations = await _organisationService.GetOrganisationsAsync();

            return View(statements);
        }
        //----------------------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();

            var today = DateTime.Today;
            var model = new CreateStatementViewModel
            {
                StartDate = new DateTime(today.Year, today.Month, 1),
                EndDate = today
            };

            return View(model);
        }
        //----------------------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStatementViewModel model)
        {
            var forClient = model.StatementFor == "Client";

            if (forClient && model.ClientID == null)
            {
                ModelState.AddModelError(nameof(model.ClientID), "Please select a client.");
            }

            if (!forClient && model.OrganisationID == null)
            {
                ModelState.AddModelError(nameof(model.OrganisationID), "Please select an organisation.");
            }

            if (model.StartDate != null && model.EndDate != null && model.StartDate > model.EndDate)
            {
                ModelState.AddModelError(nameof(model.EndDate), "The end date must be on or after the start date.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(model);
            }

            // Only send the one that matches the chosen option - the API rejects both being set.
            var request = new CreateStatementRequestModel
            {
                ClientID = forClient ? model.ClientID : null,
                OrganisationID = forClient ? null : model.OrganisationID,
                StartDate = model.StartDate!.Value,
                EndDate = model.EndDate!.Value
            };

            var (success, error, statement) = await _statementService.CreateStatementAsync(request);

            if (!success)
            {
                ModelState.AddModelError("", error ?? "Unable to create the statement.");
                await LoadDropdownsAsync();
                return View(model);
            }

            TempData["Success"] = "Statement created.";

            return RedirectToAction(nameof(Details), new { id = statement!.StatementID });
        }
        //----------------------------------------------------------------------------------
        public async Task<IActionResult> Details(int id)
        {
            var statement = await _statementService.GetStatementAsync(id);

            if (statement == null)
            {
                return NotFound();
            }

            return View(statement);
        }
        //----------------------------------------------------------------------------------
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var pdf = await _statementService.DownloadStatementPdfAsync(id);

            if (pdf == null)
            {
                TempData["Error"] = "The statement PDF could not be generated.";
                return RedirectToAction(nameof(Index));
            }

            return File(pdf, "application/pdf", $"Statement-{id}.pdf");
        }
        //----------------------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _statementService.DeleteStatementAsync(id);

            if (!success)
            {
                TempData["Error"] = "Unable to delete the statement.";
            }

            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------
        private async Task LoadDropdownsAsync()
        {
            ViewBag.Clients = await _clientService.GetClientsAsync();
            ViewBag.Organisations = await _organisationService.GetOrganisationsAsync();
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------//