using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    public class ClientModel
    {
        public int ClientID { get; set; }

        public int? OrganisationID { get; set; }

        public string? OrganisationName { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(50)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? AlternativeNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string ClientType { get; set; } = "Individual";

        [MaxLength(255)]
        public string? StreetAddress { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(10)]
        public string? PostalCode { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}