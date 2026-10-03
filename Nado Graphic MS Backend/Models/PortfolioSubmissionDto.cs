using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models
{
    // Body of POST api/PortfolioSubmissions (multipart/form-data).
    public class CreatePortfolioSubmissionRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? JobCardID { get; set; }

        [Required]
        public IFormFile MediaFile { get; set; } = null!;
    }

    // What the API returns - the entity plus display names.
    public class PortfolioSubmissionDto
    {
        public int SubmissionID { get; set; }
        public int? JobCardID { get; set; }
        public string? JobCardReference { get; set; }
        public int SubmittedByUserID { get; set; }
        public string SubmittedByName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string FileName { get; set; } = string.Empty;
        public bool IsImage { get; set; }
    }
}