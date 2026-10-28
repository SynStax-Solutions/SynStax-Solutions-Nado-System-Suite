using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Security;
using Nado_Graphics_MS_Backend.Services;

namespace Nado_Graphics_MS_Backend.Controllers
{
    // Any logged-in user can upload and view. Only a Manager or Admin can delete.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PortfolioSubmissionsController : ControllerBase
    {
        private readonly PortfolioSubmissionService _submissionService;

        public PortfolioSubmissionsController(PortfolioSubmissionService submissionService)
        {
            _submissionService = submissionService;
        }
        //--------------------------------------------------------------------------------------------------------------
        // Optional ?jobCardId= filter to show only the submissions linked to one job card.
        [HttpGet]
        public async Task<IActionResult> GetSubmissions([FromQuery] int? jobCardId)
        {
            var submissions = await _submissionService.GetSubmissionsAsync(jobCardId);
            return Ok(submissions);
        }
        //--------------------------------------------------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubmission(int id)
        {
            var submission = await _submissionService.GetSubmissionAsync(id);

            if (submission == null)
            {
                return NotFound();
            }

            return Ok(submission);
        }
        //--------------------------------------------------------------------------------------------------------------
        // Streams the uploaded file (shown inline for images and PDFs).
        [HttpGet("{id}/file")]
        public async Task<IActionResult> GetFile(int id)
        {
            var file = await _submissionService.GetFileAsync(id);

            if (file == null)
            {
                return NotFound();
            }

            return PhysicalFile(file.Value.Path, file.Value.ContentType);
        }
        //--------------------------------------------------------------------------------------------------------------
        [HttpPost]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> CreateSubmission([FromForm] CreatePortfolioSubmissionRequest request)
        {
            
            var userIdValue = User.FindFirst(AppClaims.UserId)?.Value;

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var (success, error, submission) = await _submissionService.CreateSubmissionAsync(request, userId);

            if (!success)
            {
                return this.ApiBadRequest(error);
            }

            return CreatedAtAction(nameof(GetSubmission), new { id = submission!.SubmissionID }, submission);
        }
        //--------------------------------------------------------------------------------------------------------------
        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.ManagerOrAdmin)]
        public async Task<IActionResult> DeleteSubmission(int id)
        {
            var success = await _submissionService.DeleteSubmissionAsync(id);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------//