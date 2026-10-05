using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models
{
    //What the public contact form posts. Kept separate from Enquiry
    //because Website is a honeypot field that should never be stored or trusted,
    //only checked and discarded.
    public class EnquiryRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Service { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

           
        public bool ConsentGiven { get; set; }

        //Honeypot - real visitors will never fill this in because it's hidden by CSS.
        //A bot that submits the form by blindly filling every field will fill it in.
        public string? Website { get; set; }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------