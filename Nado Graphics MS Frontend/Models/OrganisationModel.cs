namespace Nado_Graphics_MS_Frontend.Models
{
    public class OrganisationModel
    {
        public int OrganisationID { get; set; }

        public string OrganisationName { get; set; } = string.Empty;

        public string? RegistrationNumber { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? BillingAddress { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}