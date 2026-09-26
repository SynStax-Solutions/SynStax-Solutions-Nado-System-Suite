using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models
{
    public class UserRequest
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
        //Passwords must be 8-64 characters (BCrypt only uses the first 72 bytes).
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [MaxLength(64, ErrorMessage = "Password can't be longer than 64 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string UserRole { get; set; } = string.Empty;
    }
}