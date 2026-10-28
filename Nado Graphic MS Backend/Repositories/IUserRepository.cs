using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        //excludeId lets an update ignore the user being edited.
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);

        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByVerificationTokenHashAsync(string tokenHash);

        Task<User?> GetByResetTokenHashAsync(string tokenHash);
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------