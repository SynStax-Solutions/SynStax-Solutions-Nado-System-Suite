using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Security;

namespace Nado_Graphics_MS_Backend.Services
{
    //This class creates the JWT (login token) that is given to the user when they log in.
    public class TokenService
    {
        //The below line stores the app settings.
        private readonly IConfiguration _config;

        //The constructor receives the settings using dependency injection.
        public TokenService(IConfiguration config)
        {
            _config = config;
        }
        //----------------------------------------------------------------------------------//

        //This creates a token for the user and returns it with its expiry time.
        public (string Token, DateTime ExpiresAt) CreateToken(User user)
        {
            //This sets when the token expires (8 hours by default).
            var expiresAt = DateTime.UtcNow.AddHours(_config.GetValue("Jwt:ExpiryHours", 8));

            //This is the secret key used to sign the token so it can't be changed.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            //This describes what goes inside the token.
            var descriptor = new SecurityTokenDescriptor
            {
                //This stores the user's details inside the token.
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(AppClaims.UserId, user.UserID.ToString()),
                    new Claim(AppClaims.Name, $"{user.FirstName} {user.LastName}"),
                    new Claim(AppClaims.Email, user.Email),
                    new Claim(AppClaims.Role, user.UserRole)
                }),
                Expires = expiresAt,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            //This creates the token and returns it.
            return (new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
        }
    }
}
/* References
Jones, M., Bradley, J. and Sakimura, N. (2015) RFC 7519: JSON Web Token (JWT). Internet Engineering Task Force. Available at: https://www.rfc-editor.org/rfc/rfc7519 

Microsoft (n.d.) JsonWebTokenHandler Class. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.jsonwebtokens.jsonwebtokenhandler .

Microsoft (n.d.) SecurityTokenDescriptor Class. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.securitytokendescriptor .

Microsoft (n.d.) Configure JWT bearer authentication in ASP.NET Core. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication .*/
//------------------------------ END OF FILE ------------------------------//