using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    //Contract for JobCard data access. Inherits the generic CRUD methods from IRepository<JobCard>
    public interface IJobCardRepository : IRepository<JobCard>
    {
        //------------------------------------------------------------
        //Not archived, newest first, with the supplier loaded
        Task<List<JobCard>> GetActiveAsync();

        //------------------------------------------------------------
        //Archived only, newest first, with the supplier loaded
        Task<List<JobCard>> GetArchivedAsync();

        //------------------------------------------------------------
        //Returns a single job card by ID with its supplier loaded, or null if it does not exist
        Task<JobCard?> GetWithSupplierAsync(int id);

        //------------------------------------------------------------
        //Checks whether a job card with the given reference number already exists
        Task<bool> ReferenceExistsAsync(string referenceNumber);
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------