using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    //This is the model used by the frontend when working with Job Cards. 
    public class JobCardModel
    {
        public int JobCardID { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [Required]
        public int ClientID { get; set; }

        public int? SupplierID { get; set; }

        public string? SupplierName { get; set; }

        [Required]
        [MaxLength(150)]
        public string JobTitle { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? JobDescription { get; set; }

        [Required]
        [MaxLength(50)]
        public string JobStatus { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending Payment";

        public DateTime JobDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int? PortfolioSubmissionID { get; set; }
    }
}
