using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;
using Nado_Graphics_MS_Backend.Security;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Nado_Graphics_MS_Backend.Services
{
    //This class is the service that handles user business logic.
    public class UserService
    {
        private readonly IUserRepository _users;

        //The below lines store the email service (for verification emails) and the app settings.
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        //The constructor receives the database context using dependency injection.
        public UserService(IUserRepository users, IEmailService emailService, IConfiguration config)
        {
            
            _users = users;

            //Storing the injected email service and settings.
            _emailService = emailService;
            _config = config;
        }

        //----------------------------------------------------------------------------------//
        //This gets all the users from the database.
        public async Task<List<User>> GetUsersAsync()
        {
            //Getting all user records and returning them as a list.
            return await _users.GetAllAsync();
        }

        //----------------------------------------------------------------------------------//
        //This gets one user using the user's ID.
        public async Task<User?> GetUserAsync(int id)
        {
            //Searching the database for the user with the specified ID.
            return await _users.GetByIdAsync(id);
        }

        //----------------------------------------------------------------------------------//
        //This creates a new user and returns the operation result.
        //(There is no public sign-up - only a Manager or PA/Admin can do this.)
        public async Task<ServiceResult> CreateUserAsync(UserRequest request, string? actorRole)
        {
            //This defines the user roles that are allowed in the system.
            var allowedRoles = AppRoles.All;

            //This checks whether the selected user role is allowed.
            if (!allowedRoles.Contains(request.UserRole))
            {
                //This stops the process when an invalid role is selected.
                return ServiceResult.Fail("Please select a valid role.");
            }

            //This checks whether the logged-in user may create this role
            //(e.g. a PA/Admin cannot create a Manager account).
            if (!AppRoles.CanAssign(actorRole, request.UserRole))
            {
                //This stops the process when the logged-in user's role is too low.
                return ServiceResult.Fail($"You are not allowed to create a {request.UserRole} account.");
            }

            //This cleans up the email address (removes spaces, makes it lowercase).
            var email = NormaliseEmail(request.Email);

            //This checks whether another user already uses the same email address.
            var emailExists = await _users.EmailExistsAsync(email);

            //This checks whether the email address already exists.
            if (emailExists)
            {
                //This stops the process when similar user information already exists.
                return ServiceResult.Fail("An account with this email address already exists.");
            }

            //This creates a new user using the supplied information.
            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,

                //The password is hashed with BCrypt - the plain password is never stored.
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),

                UserRole = request.UserRole,
                IsActive = true,

                //The user cannot log in until they click the link in the verification email.
                EmailVerified = false,

                CreatedAt = DateTime.Now
            };

            //This adds the new user to the db context.
            await _users.AddAsync(user);

            //This saves the new user to the database.
            await _users.SaveChangesAsync();

            //This sends the verification email to the new user.
            var sent = await SendVerificationEmailAsync(user);

            //This returns a successful result.
            return sent
                ? ServiceResult.Ok($"User created. A verification email was sent to {user.Email}.")
                : ServiceResult.Ok("User created, but the verification email could not be sent. Use 'Resend verification' on the user list.");
        }

        //----------------------------------------------------------------------------------//
        //This updates an existing user and returns the operation result.
        public async Task<ServiceResult> UpdateUserAsync(int id, UpdateUser request, int actorId, string? actorRole)
        {
            //This searches the database for the existing user.
            var user = await _users.GetByIdAsync(id);

            //This checks whether the user does not exist.
            if (user == null)
            {
                //This returns an unsuccessful result when the user is not found.
                return ServiceResult.Fail("User not found.");
            }

            //This checks whether the logged-in user is editing their own account.
            var isSelf = user.UserID == actorId;

            //You may edit yourself, or anyone with a lower role than yours (Manager can edit everyone).
            if (!isSelf && !AppRoles.CanManage(actorRole, user.UserRole))
            {
                return ServiceResult.Fail("You do not have permission to edit this account.");
            }

            //This stops users from changing their own role or deactivating themselves.
            if (isSelf && (request.UserRole != user.UserRole || !request.IsActive))
            {
                return ServiceResult.Fail("You cannot change your own role or deactivate your own account.");
            }

            //This defines the user roles that are allowed in the system.
            var allowedRoles = AppRoles.All;

            //This checks whether the selected user role is allowed.
            if (!allowedRoles.Contains(request.UserRole))
            {
                //This returns an unsuccessful result when an invalid role is selected.
                return ServiceResult.Fail("Please select a valid role.");
            }

            //This checks whether the logged-in user may give out the selected role.
            if (!isSelf && !AppRoles.CanAssign(actorRole, request.UserRole))
            {
                return ServiceResult.Fail($"You are not allowed to give someone the {request.UserRole} role.");
            }

            //This cleans up the email address (removes spaces, makes it lowercase).
            var email = NormaliseEmail(request.Email);

            //This checks whether another user already uses the same email address.
            var emailExists = await _users.EmailExistsAsync(email, id);

            //This checks whether the email address already exists.
            if (emailExists)
            {
                //This returns an unsuccessful result when similar user information already exists.
                return ServiceResult.Fail("Another account already uses this email address.");
            }

            //This checks whether the email address is being changed.
            var emailChanged = !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase);

            //This updates the user's first name.
            user.FirstName = request.FirstName.Trim();

            //This updates the user's last name.
            user.LastName = request.LastName.Trim();

            //This updates the user's email address.
            user.Email = email;

            //This updates the user's role.
            user.UserRole = request.UserRole;

            //This updates the user's active status.
            user.IsActive = request.IsActive;

            //A new email address has to be verified again before the user can log in.
            if (emailChanged)
            {
                user.EmailVerified = false;
            }

            //This saves the updated user to the database.
            await _users.SaveChangesAsync();

            //This sends a new verification email if the email address changed.
            if (emailChanged)
            {
                await SendVerificationEmailAsync(user);
                return ServiceResult.Ok("User updated. The email address changed, so a new verification email was sent.");
            }

            //This returns a successful result.
            return ServiceResult.Ok("User updated.");
        }

        //----------------------------------------------------------------------------------//
        //This deletes an existing user and returns the operation result.
        public async Task<ServiceResult> DeleteUserAsync(int id, int actorId, string? actorRole)
        {
            //This searches the database for the user using the specified ID.
            var user = await _users.GetByIdAsync(id);

            //This checks whether the user does not exist.
            if (user == null)
            {
                //This returns an unsuccessful result when the user is not found.
                return ServiceResult.Fail("User not found.");
            }

            //This stops users from deleting their own account.
            if (user.UserID == actorId)
            {
                return ServiceResult.Fail("You cannot delete your own account.");
            }

            //This checks whether the logged-in user's role is high enough to delete this user.
            if (!AppRoles.CanManage(actorRole, user.UserRole))
            {
                return ServiceResult.Fail("You do not have permission to delete this account.");
            }

            try
            {
                //This marks the user for deletion in the db context.
                _users.Remove(user);

                //This saves the changes to the database.
                await _users.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                //Happens if other records (e.g. job cards) still point to this user.
                return ServiceResult.Fail("This user is linked to other records and cannot be deleted. Deactivate the account instead.");
            }

            //This returns a successful result.
            return ServiceResult.Ok("User deleted.");
        }

        //----------------------------------------------------------------------------------//
        //This sends a fresh verification link (e.g. if the first one expired).
        public async Task<ServiceResult> ResendVerificationAsync(int id, int actorId, string? actorRole)
        {
            //This searches the database for the user using the specified ID.
            var user = await _users.GetByIdAsync(id);

            //This checks whether the user does not exist.
            if (user == null)
            {
                return ServiceResult.Fail("User not found.");
            }

            //This checks whether the logged-in user may manage this account.
            if (user.UserID != actorId && !AppRoles.CanManage(actorRole, user.UserRole))
            {
                return ServiceResult.Fail("You do not have permission to manage this account.");
            }

            //This checks whether the email address is already verified.
            if (user.EmailVerified)
            {
                return ServiceResult.Fail("This email address is already verified.");
            }

            //This sends the email and returns the result.
            return await SendVerificationEmailAsync(user)
                ? ServiceResult.Ok($"A new verification email was sent to {user.Email}.")
                : ServiceResult.Fail("The verification email could not be sent. Check the email settings.");
        }

        //----------------------------------------------------------------------------------//
        //This is called when the user clicks the link.
        //The link only works ONCE because the token is removed after it is used.
        public async Task<ServiceResult> VerifyEmailAsync(string token)
        {
            //This checks whether a token was supplied.
            if (string.IsNullOrWhiteSpace(token))
            {
                return ServiceResult.Fail("This verification link is invalid.");
            }

            //This hashes the token so it can be compared with the hash stored in the database.
            var tokenHash = HashToken(token);

            //This searches for the user who owns this token.
            var user = await _users.GetByVerificationTokenHashAsync(HashToken(token));

            //This checks whether the token does not exist (wrong, or already used).
            if (user == null)
            {
                return ServiceResult.Fail("This verification link is invalid or has already been used.");
            }

            //This checks whether the link has expired.
            if (user.VerificationTokenExpiry == null || user.VerificationTokenExpiry < DateTime.UtcNow)
            {
                return ServiceResult.Fail("This verification link has expired. Ask your manager to resend it.");
            }

            //This marks the email as verified and removes the token so the link cannot be used again.
            user.EmailVerified = true;
            user.VerificationTokenHash = null;
            user.VerificationTokenExpiry = null;

            //This saves the changes to the database.
            await _users.SaveChangesAsync();

            //This returns a successful result.
            return ServiceResult.Ok("Your email address is verified. You can now log in.");
        }

        //----------------------------------------------------------------------------------//
        //This creates a random token, stores only its hash, and emails the link to the user.
        private async Task<bool> SendVerificationEmailAsync(User user)
        {
            //This creates 32 random bytes and makes them safe to use in a URL.
            var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

            //This stores the hash of the token and when it expires.
            user.VerificationTokenHash = HashToken(token);
            user.VerificationTokenExpiry = DateTime.UtcNow.AddHours(_config.GetValue("App:VerificationLinkHours", 24));

            //This saves the token hash to the database.
            await _users.SaveChangesAsync();

            //This builds the link that points to the frontend's verify page.
            var link = $"{FrontendBaseUrl()}/Account/VerifyEmail?token={Uri.EscapeDataString(token)}";

            //This is the content of the email.
            var body = $@"
                <p>Hi {WebUtility.HtmlEncode(user.FirstName)},</p>
                <p>An account has been created for you on the Nado Graphics Management System.</p>
                <p>Please verify your email address by clicking the link below:</p>
                <p><a href=""{link}"">Verify my email address</a></p>
                <p>This link works once and expires in {_config.GetValue("App:VerificationLinkHours", 24)} hours.</p>
                <p>If you were not expecting this email, you can ignore it.</p>";

            //This sends the email and returns whether it worked.
            return await _emailService.SendAsync(user.Email, "Verify your Nado Graphics account", body);
        }

        //This changes the logged-in user's own password.
        public async Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            //This searches the database for the logged-in user.
            var user = await _users.GetByIdAsync(userId);

            //This checks whether the user does not exist.
            if (user == null)
            {
                return ServiceResult.Fail("User not found.");
            }

            //This checks the current password is correct before allowing a change.
            bool currentPasswordOk;
            try
            {
                currentPasswordOk = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
            }
            catch
            {
                //If the stored password isn't a proper hash, the check fails.
                currentPasswordOk = false;
            }

            if (!currentPasswordOk)
            {
                return ServiceResult.Fail("Your current password is incorrect.");
            }

            //This stops the user from "changing" to the same password.
            if (request.CurrentPassword == request.NewPassword)
            {
                return ServiceResult.Fail("Your new password must be different from your current password.");
            }

            //This hashes and saves the new password.
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            //This records when the password changed, so older logins stop working.
            user.PasswordChangedAt = DateTime.UtcNow;

            //This saves the changes to the database.
            await _users.SaveChangesAsync();

            //This returns a successful result.
            return ServiceResult.Ok("Your password has been changed. Please log in again with your new password.");
        }

        //----------------------------------------------------------------------------------//
        //This runs when someone asks for a password reset link on the forgot password page.
        //It always "succeeds" so nobody can use this page to find out which emails have accounts.
        public async Task<ServiceResult> ForgotPasswordAsync(string emailInput)
        {
            //This is the same message for every email, whether it has an account or not.
            const string message = "If an account exists for that email address, a password reset link has been sent to it.";

            //This cleans up the email so it matches how it is stored.
            var email = NormaliseEmail(emailInput);

            //This searches the database for the user with this email.
            var user = await _users.GetByEmailAsync(email);

            //Deactivated users or emails with no account get no email, but see the same message.
            if (user == null || !user.IsActive)
            {
                return ServiceResult.Ok(message);
            }

            //This stops the same person being sent lots of emails in a row.
            //If a link was sent in the last 2 minutes, a new one is not sent.
            var linkMinutes = ResetLinkMinutes();
            if (user.ResetTokenExpiry != null && user.ResetTokenExpiry.Value.AddMinutes(-linkMinutes) > DateTime.UtcNow.AddMinutes(-2))
            {
                return ServiceResult.Ok(message);
            }

            //This creates 32 random bytes and makes them safe to use in a URL.
            var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

            //This stores only the hash of the token and when it expires.
            //A new link replaces any older one, so only the newest link works.
            user.ResetTokenHash = HashToken(token);
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(linkMinutes);
            await _users.SaveChangesAsync();

            //This builds the link that points to the frontend's reset password page.
            var link = $"{FrontendBaseUrl()}/Account/ResetPassword?token={Uri.EscapeDataString(token)}";

            //This is the content of the email.
            var body = $@"
                <p>Hi {WebUtility.HtmlEncode(user.FirstName)},</p>
                <p>Someone asked to reset the password for your Nado Graphics Management System account.</p>
                <p><a href=""{link}"">Reset my password</a></p>
                <p>This link works once and expires in {linkMinutes} minutes.</p>
                <p>If you did not ask for this, you can ignore this email. Your password will not change.</p>";

            //This sends the email. The result is not shown to the user (same message either way).
            await _emailService.SendAsync(user.Email, "Reset your Nado Graphics password", body);

            return ServiceResult.Ok(message);
        }

        //----------------------------------------------------------------------------------//
        //This runs when the user submits a new password from the reset link.
        //The link only works ONCE because the token is removed after it is used.
        public async Task<ServiceResult> ResetPasswordAsync(string token, string newPassword)
        {
            //This is the message for a wrong, used or expired link.
            const string invalidLink = "This password reset link is invalid or has expired. Please request a new one.";

            //This checks whether a token was supplied.
            if (string.IsNullOrWhiteSpace(token))
            {
                return ServiceResult.Fail(invalidLink);
            }

            //This hashes the token so it can be compared with the hash stored in the database.
            var tokenHash = HashToken(token);

            //This searches for the user who owns this token.
            var user = await _users.GetByResetTokenHashAsync(HashToken(token));

            //This checks the token exists and has not expired.
            if (user == null || user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
            {
                return ServiceResult.Fail(invalidLink);
            }

            //This hashes and saves the new password.
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            //This records when the password changed, so any older logins stop working.
            user.PasswordChangedAt = DateTime.UtcNow;

            //This removes the token so the link cannot be used again.
            user.ResetTokenHash = null;
            user.ResetTokenExpiry = null;

            //This unlocks the account, in case it was locked from too many wrong passwords.
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            //Clicking the link proves the user owns this inbox, so the email counts as verified.
            user.EmailVerified = true;
            user.VerificationTokenHash = null;
            user.VerificationTokenExpiry = null;

            //This saves the changes to the database.
            await _users.SaveChangesAsync();

            return ServiceResult.Ok("Your password has been reset. You can now log in with your new password.");
        }

        //----------------------------------------------------------------------------------//
        //This gets the website address used in email links (e.g. https://bms.nadographics.com).
        //Program.cs already checks this is set properly in production.
        private string FrontendBaseUrl() =>
            (_config["App:FrontendBaseUrl"] ?? string.Empty).TrimEnd('/');

        //This gets how long a reset link lasts (30 minutes unless changed in the settings).
        private int ResetLinkMinutes() => _config.GetValue("App:ResetLinkMinutes", 30);

        //This turns the token into a SHA-256 hash (so the real token is never stored).
        private static string HashToken(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        //This removes spaces and makes the email lowercase so emails are compared the same way.
        private static string NormaliseEmail(string email) => email.Trim().ToLowerInvariant();
    }
}

/* References
OWASP Foundation (n.d.) Password Storage Cheat Sheet. OWASP Cheat Sheet Series. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
OWASP Foundation (n.d.) Forgot Password Cheat Sheet. OWASP Cheat Sheet Series. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Forgot_Password_Cheat_Sheet.html
OWASP Foundation (n.d.) Authorization Cheat Sheet. OWASP Cheat Sheet Series. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html
Microsoft (n.d.) RandomNumberGenerator Class. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator
Microsoft (n.d.) SHA256 Class. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256
Microsoft (n.d.) Saving Data – EF Core. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/ef/core/saving/
OWASP Foundation (n.d.) Authentication Cheat Sheet: Require Re-authentication for Sensitive Features. OWASP Cheat Sheet Series. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html
OWASP Foundation (n.d.) Password Storage Cheat Sheet. OWASP Cheat Sheet Series. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
 */
//--------------------------------------------------<>\\<>End of Program<>//<>--------------------------------------------------//