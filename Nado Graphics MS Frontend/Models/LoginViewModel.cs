using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    //This class holds what the user types on the login page.
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        //This is the page the user was trying to open before being sent to login.
        public string? ReturnUrl { get; set; }
    }
}
// ------------------------- END OF FILE ------------------------- //