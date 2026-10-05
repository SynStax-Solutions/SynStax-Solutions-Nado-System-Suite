namespace Nado_Graphics_MS_Frontend.Models
{
    public class EnquiryModel
    {
        public int EnquiryID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Service { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public bool ConsentGiven { get; set; }
        public DateTime? ConsentGivenAt { get; set; }
        public bool IsRead { get; set; }
    }
}