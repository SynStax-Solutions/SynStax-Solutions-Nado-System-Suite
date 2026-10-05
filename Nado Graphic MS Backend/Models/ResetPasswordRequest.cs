using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models
{
    //This class holds the token from the reset link and the new password.
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        //The new password follows the same 8-64 character rule as when an account is created.
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [MaxLength(64, ErrorMessage = "Password can't be longer than 64 characters.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}

// OWASP Foundation (n.d.) Forgot Password Cheat Sheet. OWASP Cheat Sheet Series. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Forgot_Password_Cheat_Sheet.html