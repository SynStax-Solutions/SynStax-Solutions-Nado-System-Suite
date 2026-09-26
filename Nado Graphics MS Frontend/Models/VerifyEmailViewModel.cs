namespace Nado_Graphics_MS_Frontend.Models
{
    //This class is used on the verify email page.
    public class VerifyEmailViewModel
    {
        //The token from the email link.
        public string Token { get; set; } = string.Empty;

        //This stays empty until the user clicks the verify button.
        public bool? Success { get; set; }

        //The message shown after verifying.
        public string? Message { get; set; }
    }
}
// ------------------------- END OF FILE ------------------------- //