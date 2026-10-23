using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public class StatementRepository : Repository<Statement>, IStatementRepository
    {
        public StatementRepository(BMSDbContext context) : base(context) { }

        //-------------------------------------------------------------------------------
        public Task<List<Statement>> GetNewestFirstAsync() =>
            _set.OrderByDescending(s => s.DateGenerated).ToListAsync();
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------