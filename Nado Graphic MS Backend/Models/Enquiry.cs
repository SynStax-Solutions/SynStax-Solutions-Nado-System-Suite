using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models
{
    public class Enquiry
    {
        [Key]
        public int EnquiryID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Service { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public bool ConsentGiven { get; set; }
        public DateTime? ConsentGivenAt { get; set; }

        public bool IsRead { get; set; }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------