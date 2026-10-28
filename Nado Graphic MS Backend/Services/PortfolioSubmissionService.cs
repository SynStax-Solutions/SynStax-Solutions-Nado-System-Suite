using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;

namespace Nado_Graphics_MS_Backend.Services
{
    // Handles uploading, listing, viewing and deleting portfolio submissions.
    public class PortfolioSubmissionService
    {
        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".webp" };
        private static readonly string[] AllowedExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".pdf" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        private readonly IPortfolioSubmissionRepository _repo;
        private readonly IWebHostEnvironment _environment;

        public PortfolioSubmissionService(IPortfolioSubmissionRepository repo, IWebHostEnvironment environment)
        {
            _repo = repo;
            _environment = environment;
        }

        // Files live outside wwwroot so they can only be reached through the authorised API endpoint.
        private string UploadFolder => Path.Combine(_environment.ContentRootPath, "Uploads", "Portfolio");
        //----------------------------------------------------------------------------------//
        public async Task<List<PortfolioSubmissionDto>> GetSubmissionsAsync(int? jobCardId)
        {
            var submissions = await _repo.GetFilteredAsync(jobCardId);
            return await ToDtosAsync(submissions);
        }
        //----------------------------------------------------------------------------------//
        public async Task<PortfolioSubmissionDto?> GetSubmissionAsync(int id)
        {
            var submission = await _repo.GetByIdAsync(id);

            if (submission == null)
            {
                return null;
            }

            return (await ToDtosAsync(new List<PortfolioSubmission> { submission })).First();
        }
        //----------------------------------------------------------------------------------//
        public async Task<(bool Success, string? Error, PortfolioSubmissionDto? Submission)> CreateSubmissionAsync(
            CreatePortfolioSubmissionRequest request, int userId)
        {
            var title = request.Title?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(title))
            {
                return (false, "A title is required.", null);
            }

            if (title.Length > 150)
            {
                return (false, "The title cannot be longer than 150 characters.", null);
            }

            if (request.Description?.Length > 1000)
            {
                return (false, "The description cannot be longer than 1000 characters.", null);
            }

            var file = request.MediaFile;

            if (file == null || file.Length == 0)
            {
                return (false, "Please choose a file to upload.", null);
            }

            if (file.Length > MaxFileSize)
            {
                return (false, "The file is too large. The maximum size is 10 MB.", null);
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                return (false, "Only PNG, JPG, GIF, WEBP and PDF files are allowed.", null);
            }

            if (request.JobCardID != null && !await _repo.JobCardExistsAsync(request.JobCardID.Value))
            {
                return (false, "The selected job card was not found.", null);
            }

            // Save the file under a generated name so uploads can never overwrite each.
            Directory.CreateDirectory(UploadFolder);
            var storedName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(UploadFolder, storedName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var submission = new PortfolioSubmission
            {
                JobCardID = request.JobCardID,
                SubmittedByUserID = userId,
                Title = title,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                DateSubmitted = DateTime.Now,
                MediaURL = $"Uploads/Portfolio/{storedName}"
            };

            try
            {
                await _repo.AddAsync(submission);
                await _repo.SaveChangesAsync();
            }
            catch
            {
                // Don't leave an orphaned file behind if the database save fails.
                File.Delete(fullPath);
                throw;
            }

            var dto = (await ToDtosAsync(new List<PortfolioSubmission> { submission })).First();
            return (true, null, dto);
        }
        //----------------------------------------------------------------------------------
        // Returns the physical path and content type so the controller can stream the file.
        public async Task<(string Path, string ContentType)?> GetFileAsync(int id)
        {
            var submission = await _repo.GetByIdAsync(id);

            if (submission == null)
            {
                return null;
            }

            var path = Path.Combine(UploadFolder, Path.GetFileName(submission.MediaURL));

            if (!File.Exists(path))
            {
                return null;
            }

            return (path, GetContentType(path));
        }
        //----------------------------------------------------------------------------------
        public async Task<bool> DeleteSubmissionAsync(int id)
        {
            var submission = await _repo.GetByIdAsync(id);

            if (submission == null)
            {
                return false;
            }

            // JobCard has an optional PortfolioSubmissionID - clear it so the delete can't hit a foreign key error.
            var linkedJobCards = await _repo.GetLinkedJobCardsAsync(id);
                

            foreach (var jobCard in linkedJobCards)
            {
                jobCard.PortfolioSubmissionID = null;
            }

            _repo.Remove(submission);
            await _repo.SaveChangesAsync();

            var path = Path.Combine(UploadFolder, Path.GetFileName(submission.MediaURL));

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return true;
        }
        //----------------------------------------------------------------------------------
        private async Task<List<PortfolioSubmissionDto>> ToDtosAsync(List<PortfolioSubmission> submissions)
        {
            var jobCardIds = submissions.Where(s => s.JobCardID != null).Select(s => s.JobCardID!.Value).Distinct().ToList();
            var userIds = submissions.Select(s => s.SubmittedByUserID).Distinct().ToList();

            var jobCards = await _repo.GetJobCardReferencesAsync(jobCardIds);

            //
            var users = await _repo.GetUserFirstNamesAsync(userIds);

            return submissions.Select(s => new PortfolioSubmissionDto
            {
                SubmissionID = s.SubmissionID,
                JobCardID = s.JobCardID,
                JobCardReference = s.JobCardID != null && jobCards.TryGetValue(s.JobCardID.Value, out var reference) ? reference : null,
                SubmittedByUserID = s.SubmittedByUserID,
                SubmittedByName = users.TryGetValue(s.SubmittedByUserID, out var name) ? name : "Unknown User",
                Title = s.Title,
                Description = s.Description,
                DateSubmitted = s.DateSubmitted,
                FileName = Path.GetFileName(s.MediaURL),
                IsImage = ImageExtensions.Contains(Path.GetExtension(s.MediaURL).ToLowerInvariant())
            }).ToList();
        }
        //----------------------------------------------------------------------------------
        private static string GetContentType(string path)
        {
            return Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------