using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    public class OrganisationsController : Controller
    {
        private readonly OrganisationService _organisationService;
        //----------------------------------------------------------------------------------//
        public OrganisationsController(OrganisationService organisationService)
        {
            _organisationService = organisationService;
        }
        //----------------------------------------------------------------------------------//
        public async Task<IActionResult> Index()
        {
            var organisations = await _organisationService.GetOrganisationsAsync();

            return View(organisations);
        }
        //----------------------------------------------------------------------------------//
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        //----------------------------------------------------------------------------------//
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrganisationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _organisationService.CreateOrganisationAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Unable to create the organisation.");

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------//
        public async Task<IActionResult> Details(int id)
        {
            var organisation = await _organisationService.GetOrganisationAsync(id);

            if (organisation == null)
            {
                return NotFound();
            }

            return View(organisation);
        }
        //----------------------------------------------------------------------------------//
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var organisation = await _organisationService.GetOrganisationAsync(id);

            if (organisation == null)
            {
                return NotFound();
            }

            return View(organisation);
        }
        //----------------------------------------------------------------------------------//
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrganisationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _organisationService.UpdateOrganisationAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Unable to update the organisation.");

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------//
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _organisationService.DeleteOrganisationAsync(id);

            if (!success)
            {
                TempData["Error"] = "Unable to delete the organisation.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//