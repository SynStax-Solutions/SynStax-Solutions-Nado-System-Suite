using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public interface IPortfolioSubmissionRepository : IRepository<PortfolioSubmission>
    {
        //------------------------------------------------------------------
        //Newest first, optionally filtered by job card.
        Task<List<PortfolioSubmission>> GetFilteredAsync(int? jobCardId);

        //------------------------------------------------------------------
        Task<bool> JobCardExistsAsync(int jobCardId);

        //------------------------------------------------------------------
        //JobCardID to ReferenceNumber for the given job cards.
        Task<Dictionary<int, string>> GetJobCardReferencesAsync(List<int> jobCardIds);

        //------------------------------------------------------------------
        //UserID to FirstName for the given users.
        Task<Dictionary<int, string>> GetUserFirstNamesAsync(List<int> userIds);

        //------------------------------------------------------------------
        //Job cards that currently point at this submission (tracked, so they can be unlinked).
        Task<List<JobCard>> GetLinkedJobCardsAsync(int submissionId);
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------