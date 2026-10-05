using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    public class ViewUserModel
    {
        public int UserID { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        //Passwords must be 8-64 characters.
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [MaxLength(64, ErrorMessage = "Password can't be longer than 64 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string UserRole { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        //This shows if the user has verified their email (used for the "Email not verified" badge).
        public bool EmailVerified { get; set; }
    }
}
// ------------------------- END OF FILE ------------------------- //