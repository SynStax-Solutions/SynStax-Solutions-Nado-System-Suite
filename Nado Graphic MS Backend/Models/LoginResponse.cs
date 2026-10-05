namespace Nado_Graphics_MS_Backend.Models
{
    //This class is sent back to the frontend after a successful login.
    public class LoginResponse
    {
        //The JWT the frontend sends with every API request.
        public string Token { get; set; } = string.Empty;

        //When the token stops working.
        public DateTime ExpiresAt { get; set; }

        public int UserID { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string UserRole { get; set; } = string.Empty;
    }
}