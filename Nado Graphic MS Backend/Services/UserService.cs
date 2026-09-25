using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Services
{
    //This class is the service that handles user business logic.
    public class UserService
    {
        //The below line stores the database context used to access the database.
        private readonly BMSDbContext _context;

        //The constructor receives the database context using dependency injection.
        public UserService(BMSDbContext context)
        {
            //Storing the injected database context.
            _context = context;
        }
        //----------------------------------------------------------------------------------//

        //This gets all the users from the database.
        public async Task<List<User>> GetUsersAsync()
        {
            //Getting all user records and returning them as a list.
            return await _context.Users.ToListAsync();
        }
        //----------------------------------------------------------------------------------//

        //This gets one user using the user's ID.
        public async Task<User?> GetUserAsync(int id)
        {
            //Searching the database for the user with the specified ID.
            return await _context.Users.FindAsync(id);
        }
        //----------------------------------------------------------------------------------//

        //This creates a new user and returns the operation result.
        public async Task<bool> CreateUserAsync(UserRequest request)
        {
            //This defines the user roles that are allowed in the system.
            var allowedRoles = new[]
            {"Manager", "PA/Admin", "Graphic Designer"};

            //This checks whether the selected user role is allowed.
            if (!allowedRoles.Contains(request.UserRole))
            {
                //This stops the process when an invalid role is selected.
                return false;
            }

            //This checks whether another user already uses the same email address.
            var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);

            //This checks whether the email address already exists.
            if (emailExists)
            {
                //This stops the process when similar user information already exists.
                return false;
            }

            //This creates a new user using the supplied information.
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                UserRole = request.UserRole,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            //This adds the new user to the db context.
            _context.Users.Add(user);

            //This saves the new user to the database.
            await _context.SaveChangesAsync();

            //This returns a successful result.
            return true;
        }
        //----------------------------------------------------------------------------------//

        //This updates an existing user and returns the operation result.
        public async Task<bool> UpdateUserAsync(int id, UpdateUser request)
        {
            //This searches the database for the existing user.
            var user = await _context.Users.FindAsync(id);

            //This checks whether the user does not exist.
            if (user == null)
            {
                //This returns an unsuccessful result when the user is not found.
                return false;
            }

            //This defines the user roles that are allowed in the system.
            var allowedRoles = new[]
            {"Manager", "PA/Admin", "Graphic Designer"};

            //This checks whether the selected user role is allowed.
            if (!allowedRoles.Contains(request.UserRole))
            {
                //This returns an unsuccessful result when an invalid role is selected.
                return false;
            }

            //This checks whether another user already uses the same email address.
            var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email && u.UserID != id);

            //This checks whether the email address already exists.
            if (emailExists)
            {
                //This returns an unsuccessful result when similar user information already exists.
                return false;
            }

            //This updates the user's first name.
            user.FirstName = request.FirstName;

            //This updates the user's last name.
            user.LastName = request.LastName;

            //This updates the user's email address.
            user.Email = request.Email;

            //This updates the user's role.
            user.UserRole = request.UserRole;

            //This updates the user's active status.
            user.IsActive = request.IsActive;

            //This saves the updated user to the database.
            await _context.SaveChangesAsync();

            //This returns a successful result.
            return true;
        }
        //----------------------------------------------------------------------------------//

        //This deletes an existing user and returns the operation result.
        public async Task<bool> DeleteUserAsync(int id)
        {
            //This searches the database for the user using the specified ID.
            var user = await _context.Users.FindAsync(id);

            //This checks whether the user does not exist.
            if (user == null)
            {
                //This returns an unsuccessful result when the user is not found.
                return false;
            }

            //This marks the user for deletion in the db context.
            _context.Users.Remove(user);

            //This saves the changes to the database.
            await _context.SaveChangesAsync();

            //This returns a successful result.
            return true;
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//