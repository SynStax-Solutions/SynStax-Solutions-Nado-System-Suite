using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Frontend.Models;
using Nado_Graphics_MS_Frontend.Security;
using Nado_Graphics_MS_Frontend.Services;

namespace Nado_Graphics_MS_Frontend.Controllers
{
    // Any logged-in user can view and upload. Only a Manager or Admin can delete.
    [Authorize]
    public class PortfolioController : Controller
    {
        private static readonly string[] AllowedExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".pdf" };
        private const long MaxFileSize = 10 * 1024 * 1024;

        private readonly PortfolioService _portfolioService;
        private readonly JobCardService _jobCardService;
        //----------------------------------------------------------------------------------
        public PortfolioController(PortfolioService portfolioService, JobCardService jobCardService)
        {
            _portfolioService = portfolioService;
            _jobCardService = jobCardService;
        }
        //----------------------------------------------------------------------------------
        public async Task<IActionResult> Index(int? jobCardId)
        {
            var submissions = await _portfolioService.GetSubmissionsAsync(jobCardId);
            ViewBag.JobCardId = jobCardId;

            return View(submissions);
        }
        //----------------------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Create(int? jobCardId)
        {
            await LoadJobCardsAsync();

            return View(new CreatePortfolioSubmissionViewModel { JobCardID = jobCardId });
        }
        //----------------------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> Create(CreatePortfolioSubmissionViewModel model)
        {
            if (model.MediaFile != null)
            {
                var extension = Path.GetExtension(model.MediaFile.FileName).ToLowerInvariant();

                if (!AllowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.MediaFile), "Only PNG, JPG, GIF, WEBP and PDF files are allowed.");
                }
                else if (model.MediaFile.Length > MaxFileSize)
                {
                    ModelState.AddModelError(nameof(model.MediaFile), "The file is too large. The maximum size is 10 MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadJobCardsAsync();
                return View(model);
            }

            var (success, error) = await _portfolioService.CreateSubmissionAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", error ?? "Unable to upload the file.");
                await LoadJobCardsAsync();
                return View(model);
            }

            TempData["Success"] = "Portfolio submission uploaded.";

            return RedirectToAction(nameof(Index), new { jobCardId = model.JobCardID });
        }
        //----------------------------------------------------------------------------------
        // Serves the image / PDF through the logged-in session so the file is never publicly reachable.
        public async Task<IActionResult> Media(int id)
        {
            var result = await _portfolioService.GetFileAsync(id);

            if (result == null)
            {
                return NotFound();
            }

            return File(result.Value.Bytes, result.Value.ContentType);
        }
        //----------------------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.ManagerOrAdmin)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _portfolioService.DeleteSubmissionAsync(id);

            TempData[success ? "Success" : "Error"] = success ? "Submission deleted." : "Unable to delete the submission.";

            return RedirectToAction(nameof(Index));
        }
        //----------------------------------------------------------------------------------
        private async Task LoadJobCardsAsync()
        {
            ViewBag.JobCards = await _jobCardService.GetJobCardsAsync();
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------//