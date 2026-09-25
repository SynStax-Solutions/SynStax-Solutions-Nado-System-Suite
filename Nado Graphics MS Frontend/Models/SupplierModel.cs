using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    public class SupplierModel
    {
        public int SupplierID { get; set; }

        [Required]
        [MaxLength(50)]
        public string SupplierName { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? ContactNumber { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        public string? SupplierEmail { get; set; }

        [MaxLength(255)]
        public string? SupplierAddress { get; set; }

        [Required]
        [MaxLength(255)]
        public string LeadTime { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string ServiceType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}