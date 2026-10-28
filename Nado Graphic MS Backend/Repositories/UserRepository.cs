using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        //-----------------------------------------------------------------------------------
        public UserRepository(BMSDbContext context) : base(context) { }

        //-----------------------------------------------------------------------------------
        public Task<bool> EmailExistsAsync(string email, int? excludeId = null) =>
            _set.AnyAsync(u => u.Email == email && (excludeId == null || u.UserID != excludeId));

        //-----------------------------------------------------------------------------------
        public Task<User?> GetByEmailAsync(string email) =>
            _set.FirstOrDefaultAsync(u => u.Email == email);

        //-----------------------------------------------------------------------------------
        public Task<User?> GetByVerificationTokenHashAsync(string tokenHash) =>
            _set.FirstOrDefaultAsync(u => u.VerificationTokenHash == tokenHash);

        //-----------------------------------------------------------------------------------
        public Task<User?> GetByResetTokenHashAsync(string tokenHash) =>
            _set.FirstOrDefaultAsync(u => u.ResetTokenHash == tokenHash);
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------