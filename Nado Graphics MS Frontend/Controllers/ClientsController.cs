using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ClientService _clientService;
        private readonly OrganisationService _organisationService;
        //----------------------------------------------------------------------------------//
        public ClientsController(ClientService clientService, OrganisationService organisationService)
        {
            _clientService = clientService;
            _organisationService = organisationService;
        }
        //----------------------------------------------------------------------------------//

        public async Task<IActionResult> Index()
        {
            var clients = await _clientService.GetClientsAsync();

            return View(clients);
        }
        //----------------------------------------------------------------------------------//
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Organisations = await _organisationService.GetOrganisationsAsync();

            return View();
        }
        //----------------------------------------------------------------------------------//

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Organisations = await _organisationService.GetOrganisationsAsync();

                return View(model);
            }

            var success = await _clientService.CreateClientAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Unable to create the client.");

                ViewBag.Organisations = await _organisationService.GetOrganisationsAsync();

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------//

        public async Task<IActionResult> Details(int id)
        {
            var client = await _clientService.GetClientAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }
        //----------------------------------------------------------------------------------//

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientService.GetClientAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            var organisations = await _organisationService.GetOrganisationsAsync();

            ViewBag.Organisations = organisations;

            return View(client);
        }
        //----------------------------------------------------------------------------------//

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Organisations = await _organisationService.GetOrganisationsAsync();

                return View(model);
            }

            var success = await _clientService.UpdateClientAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Unable to update the client.");

                ViewBag.Organisations = await _organisationService.GetOrganisationsAsync();

                return View(model);
            }

            return Ok(model);
        }
        //----------------------------------------------------------------------------------//

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _clientService.DeleteClientAsync(id);

            if (!success)
            {
                TempData["Error"] = "Unable to delete the client.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//