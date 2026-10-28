using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    //Contract for Organisation data access. Inherits the generic CRUD methods from IRepository<Organisation>
    public interface IOrganisationRepository : IRepository<Organisation>
    {
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------