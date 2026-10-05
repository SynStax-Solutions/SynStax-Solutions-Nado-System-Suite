namespace Nado_Graphics_MS_Frontend.Models
{
    //This class holds what the API sends back after logging in.
    public class LoginResponseModel
    {
        //The JWT used for API requests.
        public string Token { get; set; } = string.Empty;

        //When the token expires.
        public DateTime ExpiresAt { get; set; }

        public int UserID { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string UserRole { get; set; } = string.Empty;
    }
}
// ------------------------- END OF FILE ------------------------- //