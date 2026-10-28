using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public interface IClientRepository : IRepository<Client>
    {
        //IDs of every client that belongs to an organisation
        Task<List<int>> GetIdsByOrganisationAsync(int organisationId);

        //ClientID to "First Last" for the given clients
        Task<Dictionary<int, string>> GetFullNamesByIdsAsync(List<int> clientIds);
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------