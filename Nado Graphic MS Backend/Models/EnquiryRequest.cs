namespace Nado_Graphics_MS_Backend.Models
{
    //What the public contact form actually posts. Kept separate from Enquiry
    //because Website is a honeypot field that must never be stored or trusted,
    //only checked and discarded.
    public class EnquiryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Service { get; set; }
        public string Message { get; set; } = string.Empty;

        //Honeypot - real visitors never fill this in, since it's hidden by CSS.
        //A bot that submits the form by blindly filling every field will fill it.
        public string? Website { get; set; }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------//