using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    //This class holds what the user types on the forgot password page.
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;
    }
}

// ------------------------- END OF FILE ------------------------- //