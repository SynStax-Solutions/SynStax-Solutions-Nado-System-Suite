using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models
{
    //This class holds the details sent from the change password page.
    public class ChangePasswordRequest
    {
        //The user's current password, to prove it's really them.
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        //The new password. It follows the same 8-64 character rule as when an account is created.
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [MaxLength(64, ErrorMessage = "Password can't be longer than 64 characters.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
// OWASP Foundation (n.d.) Authentication Cheat Sheet: Require Re-authentication for Sensitive Features. OWASP Cheat Sheet Series. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html