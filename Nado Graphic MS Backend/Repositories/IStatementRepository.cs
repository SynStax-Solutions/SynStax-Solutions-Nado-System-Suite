using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public interface IStatementRepository : IRepository<Statement>
    {
        Task<List<Statement>> GetNewestFirstAsync();
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------