using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models
{
    public class Organisation
    {
        [Key]
        public int OrganisationID { get; set; }

        [Required]
        [MaxLength(150)]
        public string OrganisationName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? RegistrationNumber { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? BillingAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}