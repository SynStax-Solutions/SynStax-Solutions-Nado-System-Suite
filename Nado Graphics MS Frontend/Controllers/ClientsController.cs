using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Nado_Graphics_MS_Frontend.Security;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    //This controller handles Client pages and Client-related actions.
    //Only a logged-in Manager (Owner) or PA/Admin can open these pages.
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class ClientsController : Controller
    {
        //This stores the service used to manage Clients.
        private readonly ClientService _clientService;

        //This stores the service used to retrieve Organisations.
        private readonly OrganisationService _organisationService;
        //----------------------------------------------------------------------------------//
        //This receives both services using dependency injection.
        public ClientsController(ClientService clientService, OrganisationService organisationService)
        {
            _clientService = clientService;
            _organisationService = organisationService;
        }
        //----------------------------------------------------------------------------------//

        //This displays the list of Clients.
        public async Task<IActionResult> Index()
        {
            //This gets all Clients from the Client service.
            var clients = await _clientService.GetClientsAsync();

            //This sends the Clients to the Index view.
            return View(clients);
        }
        //----------------------------------------------------------------------------------//

        //This displays the Create Client page.
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            //This gets all Organisations so one can be selected for the Client.
            ViewBag.Organisations = await _organisationService.GetOrganisationsAsync();

            //This returns the Create Client view.
            return View();
        }
        //----------------------------------------------------------------------------------//

        //This processes the submitted Create Client form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientModel model)
        {
            //This checks whether the submitted Client information passes validation.
            if (!ModelState.IsValid)
            {
                //This reloads the Organisations when validation fails.
                ViewBag.Organisations = await _organisationService.GetOrganisationsAsync();

                //This returns the user to the form with the submitted values.
                return View(model);
            }

            //This sends the Client to the service for creation.
            var success = await _clientService.CreateClientAsync(model);

            //This checks whether the Client could not be created.
            if (!success)
            {
                //This displays an error message on the form.
                ModelState.AddModelError("", "Unable to create the client.");

                //This reloads the Organisations for the form.
                ViewBag.Organisations = await _organisationService.GetOrganisationsAsync();

                //This returns the user to the form.
                return View(model);
            }

            //This returns to the Client list after successful creation.
            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------//

        //This displays the details of one Client.
        public async Task<IActionResult> Details(int id)
        {
            //This gets the selected Client from the Client service.
            var client = await _clientService.GetClientAsync(id);

            //This checks whether the Client does not exist.
            if (client == null)
            {
                //This returns a 404 response when the Client cannot be found.
                return NotFound();
            }

            //This sends the Client to the Details view.
            return View(client);
        }
        //----------------------------------------------------------------------------------//

        //This displays the Edit Client page.
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            //This gets the selected Client from the Client service.
            var client = await _clientService.GetClientAsync(id);

            //This checks whether the Client does not exist.
            if (client == null)
            {
                //This returns a 404 response when the Client cannot be found.
                return NotFound();
            }

            //This gets all Organisations that can be assigned to the Client.
            var organisations = await _organisationService.GetOrganisationsAsync();

            //This makes the Organisations available to the Edit view.
            ViewBag.Organisations = organisations;

            //This sends the Client to the Edit view.
            return View(client);
        }
        //----------------------------------------------------------------------------------//

        //This processes the submitted Edit Client form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientModel model)
        {
            //This checks whether the submitted Client information passes validation.
            if (!ModelState.IsValid)
            {
                //This reloads the Organisations when validation fails.
                ViewBag.Organisations = await _organisationService.GetOrganisationsAsync();

                //This returns the user to the form with the submitted values.
                return View(model);
            }

            //This sends the updated Client to the service.
            var success = await _clientService.UpdateClientAsync(model);

            //This checks whether the Client could not be updated.
            if (!success)
            {
                //This displays an error message on the form.
                ModelState.AddModelError("", "Unable to update the client.");

                //This reloads the Organisations for the form.
                ViewBag.Organisations = await _organisationService.GetOrganisationsAsync();

                //This returns the user to the form.
                return View(model);
            }

            //This redirects to the updated Client's Details page.
            return RedirectToAction(nameof(Details), new { id = model.ClientID });
        }
        //----------------------------------------------------------------------------------//

        //This deletes an existing Client.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            //This asks the Client service to delete the selected Client.
            var success = await _clientService.DeleteClientAsync(id);

            //This checks whether the Client could not be deleted.
            if (!success)
            {
                //This stores an error message for the next page request.
                TempData["Error"] = "Unable to delete the client.";
            }

            //This returns to the Client list.
            return RedirectToAction(nameof(Index));
        }
    }
}

//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//