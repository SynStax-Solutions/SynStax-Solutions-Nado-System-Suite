using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Nado_Graphics_MS_Frontend.Security;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    //This controller handles Organisation pages and Organisation-related actions.
    //Only a logged-in Manager (Owner) or PA/Admin can open these pages.
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class OrganisationsController : Controller
    {
        //This stores the service used to manage Organisations.
        private readonly OrganisationService _organisationService;
        //----------------------------------------------------------------------------------//
        //This receives the Organisation service using dependency injection.
        public OrganisationsController(OrganisationService organisationService)
        {
            _organisationService = organisationService;
        }
        //----------------------------------------------------------------------------------//

        //This displays the list of Organisations.
        public async Task<IActionResult> Index()
        {
            //This gets all Organisations from the Organisation service.
            var organisations = await _organisationService.GetOrganisationsAsync();

            //This sends the Organisations to the Index view.
            return View(organisations);
        }
        //----------------------------------------------------------------------------------//

        //This displays the Create Organisation page.
        [HttpGet]
        public IActionResult Create()
        {
            //This returns the empty Create Organisation form.
            return View();
        }
        //----------------------------------------------------------------------------------//

        //This processes the submitted Create Organisation form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrganisationModel model)
        {
            //This checks whether the submitted Organisation information passes validation.
            if (!ModelState.IsValid)
            {
                //This returns the form with the submitted values when validation fails.
                return View(model);
            }

            //This sends the Organisation to the service for creation.
            var success = await _organisationService.CreateOrganisationAsync(model);

            //This checks whether the Organisation could not be created.
            if (!success)
            {
                //This displays an error message on the form.
                ModelState.AddModelError("", "Unable to create the organisation.");

                //This returns the user to the form.
                return View(model);
            }

            //This returns to the Organisation list after successful creation.
            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------//

        //This displays the details of one Organisation.
        public async Task<IActionResult> Details(int id)
        {
            //This gets the selected Organisation from the Organisation service.
            var organisation = await _organisationService.GetOrganisationAsync(id);

            //This checks whether the Organisation does not exist.
            if (organisation == null)
            {
                //This returns a 404 response when the Organisation cannot be found.
                return NotFound();
            }

            //This sends the Organisation to the Details view.
            return View(organisation);
        }
        //----------------------------------------------------------------------------------//

        //This displays the Edit Organisation page.
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            //This gets the selected Organisation from the Organisation service.
            var organisation = await _organisationService.GetOrganisationAsync(id);

            //This checks whether the Organisation does not exist.
            if (organisation == null)
            {
                //This returns a 404 response when the Organisation cannot be found.
                return NotFound();
            }

            //This sends the Organisation to the Edit view.
            return View(organisation);
        }
        //----------------------------------------------------------------------------------//

        //This processes the submitted Edit Organisation form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrganisationModel model)
        {
            //This checks whether the submitted Organisation information passes validation.
            if (!ModelState.IsValid)
            {
                //This returns the form with the submitted values when validation fails.
                return View(model);
            }

            //This sends the updated Organisation to the service.
            var success = await _organisationService.UpdateOrganisationAsync(model);

            //This checks whether the Organisation could not be updated.
            if (!success)
            {
                //This displays an error message on the form.
                ModelState.AddModelError("", "Unable to update the organisation.");

                //This returns the user to the form.
                return View(model);
            }

            //This returns to the Organisation list after a successful update.
            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------//

        //This deletes an existing Organisation.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            //This asks the Organisation service to delete the selected Organisation.
            var success = await _organisationService.DeleteOrganisationAsync(id);

            //This checks whether the Organisation could not be deleted.
            if (!success)
            {
                //This stores an error message for the next page request.
                TempData["Error"] = "Unable to delete the organisation.";
            }

            //This returns to the Organisation list.
            return RedirectToAction(nameof(Index));
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//