using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    //This class holds what the user types on the change password page.
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Please enter your current password.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        //The new password follows the same 8-64 character rule as the backend.
        [Required(ErrorMessage = "Please enter a new password.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [MaxLength(64, ErrorMessage = "Password can't be longer than 64 characters.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        //This makes the user type the new password twice, so a typo doesn't lock them out.
        [Required(ErrorMessage = "Please confirm your new password.")]
        [Compare(nameof(NewPassword), ErrorMessage = "The passwords don't match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}