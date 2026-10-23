using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    //Data access for Enquiry records
    public class EnquiryRepository : Repository<Enquiry>, IEnquiryRepository
    {
        //The DbContext is injected by the DI container and passed to the base repository
        public EnquiryRepository(BMSDbContext context) : base(context) { }

        //----------------------------------------------------------------------------------------------
        //Returns all enquiries sorted by submission date, most recent first
        public Task<List<Enquiry>> GetNewestFirstAsync() =>
            _set.OrderByDescending(e => e.SubmittedAt).ToListAsync();
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------