using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public class OrganisationRepository : Repository<Organisation>, IOrganisationRepository
    {
        public OrganisationRepository(BMSDbContext context) : base(context) { }

        //----------------------------------------------------------------------------------------------------------
        public Task<bool> NameExistsAsync(string name, int? excludeId = null) =>
            _set.AnyAsync(o => o.OrganisationName == name && (excludeId == null || o.OrganisationID != excludeId));
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------