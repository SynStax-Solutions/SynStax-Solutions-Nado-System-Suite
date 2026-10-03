using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models
{
    // EF Core entity mapped to dbo.PortfolioSubmission.
    public class PortfolioSubmission
    {
        [Key]
        public int SubmissionID { get; set; }

        // Optional - a submission can be linked to a job card or stand on its own.
        public int? JobCardID { get; set; }

        [Required]
        public int SubmittedByUserID { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        // Relative path of the saved file, e.g. "Uploads/Portfolio/3f2a....png"
        [Required]
        [MaxLength(255)]
        public string MediaURL { get; set; } = string.Empty;
    }
}