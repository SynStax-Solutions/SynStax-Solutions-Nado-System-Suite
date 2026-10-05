using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

        [Required]
        [MaxLength(50)]
        public string SupplierName { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? ContactNumber { get; set; }

        [MaxLength(255)]
        [EmailAddress]
        public string? SupplierEmail { get; set; }

        [MaxLength(255)]
        public string? SupplierAddress { get; set; }

        [Required]
        [MaxLength(255)]
        public string LeadTime { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string ServiceType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public decimal? ServiceRate { get; set; }
    }
}