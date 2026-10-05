using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    // Mirrors the DTO returned by the API.
    public class PortfolioSubmissionModel
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

    // What the upload form binds to.
    public class CreatePortfolioSubmissionViewModel
    {
        [Required(ErrorMessage = "Please enter a title.")]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        // Optional - leave empty for a standalone submission.
        public int? JobCardID { get; set; }

        [Required(ErrorMessage = "Please choose a file.")]
        public IFormFile? MediaFile { get; set; }
    }
}