using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    //this is not a final model
    public class JobCardModel
    {
        public int JobCardID { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [Required]
        public int ClientID { get; set; }

        [Required]
        [MaxLength(150)]
        public string JobTitle { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? JobDescription { get; set; }

        [Required]
        [MaxLength(50)]
        public string JobStatus { get; set; } = string.Empty;

        public DateTime JobDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int? PortfolioSubmissionID { get; set; }
    }
}
