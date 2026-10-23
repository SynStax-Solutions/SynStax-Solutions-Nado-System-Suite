using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    //Data access for Client records
    //Inherits the CRUD methods from Repository<Client>
    //adds client-specific queries
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        
        //The DbContext is injected by the DI container and passed to the base repository
        public ClientRepository(BMSDbContext context) : base(context) { }


        //----------------------------------------------------------------------------------------------
        //Returns the IDs of all clients that belong to the given organisation
        public Task<List<int>> GetIdsByOrganisationAsync(int organisationId) =>
            _set.Where(c => c.OrganisationID == organisationId)
                .Select(c => c.ClientID)
                .ToListAsync();

        //----------------------------------------------------------------------------------------------
        //Looks up the clients with the given IDs and returns a dictionary of ClientID to "FirstName LastName"
        public Task<Dictionary<int, string>> GetFullNamesByIdsAsync(List<int> clientIds) =>
            _set.Where(c => clientIds.Contains(c.ClientID))
                .ToDictionaryAsync(c => c.ClientID, c => $"{c.FirstName} {c.LastName}");
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------