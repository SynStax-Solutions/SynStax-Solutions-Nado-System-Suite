using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    //Any logged-in user can open these pages (all roles work with job cards). 
    [Authorize]
    public class JobCardsController : Controller
    {
        //The below line stores the JobCardService. 
        private readonly JobCardService _jobCardService;
        private readonly PortfolioService _portfolioService;

        //The constructor receives the JobCardService using dependency injection. 
        public JobCardsController(JobCardService jobCardService, PortfolioService portfolioService)
        {
            _jobCardService = jobCardService;
            _portfolioService = portfolioService;
        }

        //----------------------------------------------------------------------------------// 

        //This gets all Job Cards and sends them to the Index view. 
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //This gets the Job Cards from the backend API. 
            var jobCards = await _jobCardService.GetJobCardsAsync();

            //This sends the Job Cards to the page. 
            return View(jobCards);
        }
        //---------------------------------------------------------------------------------------------------------------------// 

        [HttpGet]
        public async Task<IActionResult> Archived()
        {
            var jobCards = await _jobCardService.GetArchivedJobCardsAsync();

            return View(jobCards);
        }
        //----------------------------------------------------------------------------------// 

        //This gets one Job Card and sends it to the Details view. 
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            //This gets the selected Job Card from the backend API. 
            var jobCard = await _jobCardService.GetJobCardAsync(id);

            //This checks whether the Job Card exists. 
            if (jobCard == null)
            {
                return NotFound();
            }
            var suppliers = await _jobCardService.GetSuppliersAsync();

            ViewBag.Suppliers = suppliers;

            //Submissions linked to this specific job card
            ViewBag.PortfolioSubmissions = await _portfolioService.GetSubmissionsAsync(id);

            //This sends the Job Card to the page. 
            return View(jobCard);
        }

        //----------------------------------------------------------------------------------// 

        //This assigns a supplier to the Job Card. 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignSupplier(int id, int supplierId)
        {
            //This checks that a supplier was selected. 
            if (supplierId <= 0)
            {
                TempData["Error"] = "Please select a supplier.";
                return RedirectToAction(nameof(Details), new { id });
            }

            //This sends the supplier assignment to the API. 
            var success = await _jobCardService.AssignSupplierAsync(id, supplierId);

            //This shows an error if the assignment failed. 
            if (!success)
            {
                TempData["Error"] = "Unable to assign the supplier.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        //----------------------------------------------------------------------------------// 

        //This gets one Job Card and sends it to the Edit view. 
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            //This gets the selected Job Card from the backend API. 
            var jobCard = await _jobCardService.GetJobCardAsync(id);

            //This checks whether the Job Card exists. 
            if (jobCard == null)
            {
                return NotFound();
            }

            //This sends the Job Card to the Edit page. 
            return View(jobCard);
        }

        //----------------------------------------------------------------------------------// 

        //This saves the edited Job Card. 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(JobCardModel model)
        {
            //This checks whether the form values are valid. 
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //This sends the updated Job Card to the API. 
            var success = await _jobCardService.UpdateJobCardAsync(model);

            //This shows an error if the update failed. 
            if (!success)
            {
                ModelState.AddModelError("", "Unable to update the job card.");
                return View(model);
            }

            //This returns to the Job Card details page after saving. 
            return RedirectToAction(nameof(Details), new { id = model.JobCardID });
        }

        //-----------------------------------------------------------------------------------------------// 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var success = await _jobCardService.UpdateStatusAsync(id, status);

            if (!success)
            {
                TempData["Error"] = "Unable to update the job card status.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}