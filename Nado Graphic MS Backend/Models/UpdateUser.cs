using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models
{
    public class UpdateUser
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string UserRole { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}