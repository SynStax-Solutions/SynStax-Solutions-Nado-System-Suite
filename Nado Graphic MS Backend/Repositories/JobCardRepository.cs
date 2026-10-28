using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public class JobCardRepository : Repository<JobCard>, IJobCardRepository
    {
        public JobCardRepository(BMSDbContext context) : base(context) { }

        //-------------------------------------------------------
        public Task<List<JobCard>> GetActiveAsync() =>
            _set.Include(j => j.Supplier)
                .Where(j => j.JobStatus != "Archived")
                .OrderByDescending(j => j.JobDate)
                .ToListAsync();
        //-------------------------------------------------------
        public Task<List<JobCard>> GetArchivedAsync() =>
            _set.Include(j => j.Supplier)
                .Where(j => j.JobStatus == "Archived")
                .OrderByDescending(j => j.JobDate)
                .ToListAsync();
        //-------------------------------------------------------
        public Task<JobCard?> GetWithSupplierAsync(int id) =>
            _set.Include(j => j.Supplier).FirstOrDefaultAsync(j => j.JobCardID == id);
        
        //-------------------------------------------------------
        public Task<bool> ReferenceExistsAsync(string referenceNumber) =>
            _set.AnyAsync(j => j.ReferenceNumber == referenceNumber);
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------