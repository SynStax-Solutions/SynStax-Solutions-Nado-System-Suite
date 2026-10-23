using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public class PortfolioSubmissionRepository : Repository<PortfolioSubmission>, IPortfolioSubmissionRepository
    {
        public PortfolioSubmissionRepository(BMSDbContext context) : base(context) { }

        //-----------------------------------------------------------------------------------
        public Task<List<PortfolioSubmission>> GetFilteredAsync(int? jobCardId)
        {
            var query = _set.AsQueryable();

            if (jobCardId != null)
            {
                query = query.Where(s => s.JobCardID == jobCardId);
            }

            return query.OrderByDescending(s => s.DateSubmitted).ToListAsync();
        }

        //-----------------------------------------------------------------------------------
        public Task<bool> JobCardExistsAsync(int jobCardId) =>
            _context.JobCards.AnyAsync(j => j.JobCardID == jobCardId);

        //-----------------------------------------------------------------------------------
        public Task<Dictionary<int, string>> GetJobCardReferencesAsync(List<int> jobCardIds) =>
            _context.JobCards
                .Where(j => jobCardIds.Contains(j.JobCardID))
                .ToDictionaryAsync(j => j.JobCardID, j => j.ReferenceNumber);

        //-----------------------------------------------------------------------------------
        public Task<Dictionary<int, string>> GetUserFirstNamesAsync(List<int> userIds) =>
            _context.Users
                .Where(u => userIds.Contains(u.UserID))
                .ToDictionaryAsync(u => u.UserID, u => u.FirstName);

        //-----------------------------------------------------------------------------------
        public Task<List<JobCard>> GetLinkedJobCardsAsync(int submissionId) =>
            _context.JobCards
                .Where(j => j.PortfolioSubmissionID == submissionId)
                .ToListAsync();
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------