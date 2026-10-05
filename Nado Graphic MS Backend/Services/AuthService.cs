using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Services
{
    //This class handles the login logic.
    public class AuthService
    {
        //After this many wrong passwords in a row, the account is locked.
        private const int MaxFailedAttempts = 5;

        //How long the account stays locked.
        private const int LockoutMinutes = 15;

        //This fake hash is checked when the email doesn't exist,
        //so a wrong email takes the same time as a wrong password.
        private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword("dummy-password-for-timing");

        //The below lines store the database context and the token service.
        private readonly BMSDbContext _context;
        private readonly TokenService _tokenService;

        //The constructor receives both using dependency injection.
        public AuthService(BMSDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }
        //----------------------------------------------------------------------------------//

        //This checks the login details and returns the user's info and token if they are correct.
        public async Task<(LoginResponse? Response, int StatusCode, string? Error)> LoginAsync(LoginRequest request)
        {
            //The same message is used for a wrong email or wrong password.
            const string invalidLogin = "Invalid email or password.";

            //This cleans up the email so it matches how it is stored.
            var email = request.Email.Trim().ToLowerInvariant();

            //This searches the database for the user with this email.
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            //This stops the login if the account is currently locked.
            if (user?.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
            {
                return (null, StatusCodes.Status403Forbidden, LockedMessage(user.LockoutEnd.Value));
            }

            //This checks the typed password against the hashed password in the database.
            var passwordOk = VerifyPassword(request.Password, user?.PasswordHash ?? DummyHash);

            //This rejects the login if the user doesn't exist.
            if (user == null)
            {
                return (null, StatusCodes.Status401Unauthorized, invalidLogin);
            }

            //This runs when the password is wrong.
            if (!passwordOk)
            {
                //This adds 1 to the wrong password count.
                user.FailedLoginAttempts++;

                //If there were too many wrong passwords, the account is locked and the count starts again.
                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                    user.FailedLoginAttempts = 0;
                    await _context.SaveChangesAsync();

                    return (null, StatusCodes.Status403Forbidden, LockedMessage(user.LockoutEnd.Value));
                }

                //This saves the new count.
                await _context.SaveChangesAsync();

                return (null, StatusCodes.Status401Unauthorized, invalidLogin);
            }

            //The password was right, so the wrong password count and lock are cleared.
            if (user.FailedLoginAttempts > 0 || user.LockoutEnd != null)
            {
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                await _context.SaveChangesAsync();
            }

            //This stops users who have not verified their email yet.
            if (!user.EmailVerified)
            {
                return (null, StatusCodes.Status403Forbidden,
                    "Please verify your email address first. Check your inbox for the verification link.");
            }

            //This stops users whose account has been deactivated.
            if (!user.IsActive)
            {
                return (null, StatusCodes.Status403Forbidden,
                    "This account has been deactivated. Please contact your manager.");
            }

            //This creates the login token for the user.
            var (token, expiresAt) = _tokenService.CreateToken(user);

            //This returns the token and the user's details.
            return (new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserRole = user.UserRole
            }, StatusCodes.Status200OK, null);
        }
        //----------------------------------------------------------------------------------//

        //This builds the message that says how long the account is locked for.
        private static string LockedMessage(DateTime lockoutEnd)
        {
            var minutesLeft = (int)Math.Ceiling((lockoutEnd - DateTime.UtcNow).TotalMinutes);
            return $"Too many failed login attempts. Please try again in {minutesLeft} minute(s).";
        }
        //----------------------------------------------------------------------------------//

        //This checks if the password matches the hash.
        private static bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                //If the stored password isn't a proper hash (e.g. plain text), the login fails.
                return false;
            }
        }
    }
}
/* References 
OWASP Foundation (n.d.) Authentication Cheat Sheet. OWASP Cheat Sheet Series. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html .

OWASP Foundation (n.d.) Password Storage Cheat Sheet. OWASP Cheat Sheet Series. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html .

BCrypt.Net-Next (n.d.) BCrypt.Net-Next. GitHub. Available at: https://github.com/BcryptNet/bcrypt.net .

OWASP Foundation (n.d.) Authentication Cheat Sheet: Account Lockout. OWASP Cheat Sheet Series. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html
 */
//------------------------------ END OF FILE ------------------------------//