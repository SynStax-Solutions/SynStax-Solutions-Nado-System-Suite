using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    //Contract for Enquiry data access
    public interface IEnquiryRepository : IRepository<Enquiry>
    {
        //-------------------------------------------------------------------
        //Returns all enquiries sorted by submission date, most recent first
        Task<List<Enquiry>> GetNewestFirstAsync();
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------