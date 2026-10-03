using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;
using Microsoft.AspNetCore.SignalR;
using Nado_Graphics_MS_Backend.Hubs;

namespace Nado_Graphics_MS_Backend.Services
{
    //This class is the service that handles job card business logic. 
    public class JobCardService
    {
        //The below line stores the database context used to access the database. 
        private readonly BMSDbContext _context;
        private readonly IHubContext<JobCardHub> _hubContext;

        //The constructor receives the database context using dependency injection. 
        public JobCardService(BMSDbContext context, IHubContext<JobCardHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        //----------------------------------------------------------------------------------// 

        //This gets all job cards from the database, newest first. 
        public async Task<List<JobCard>> GetJobCardsAsync()
        {
            return await _context.JobCards
                .Include(j => j.Supplier)
                .Where(j => j.JobStatus != "Archived")
                .OrderByDescending(j => j.JobDate)
                .ToListAsync();
        }
        //-------------------------------------------------------------------------------------------// 

        public async Task<List<JobCard>> GetArchivedJobCardsAsync()
        {
            return await _context.JobCards
                .Include(j => j.Supplier)
                .Where(j => j.JobStatus == "Archived")
                .OrderByDescending(j => j.JobDate)
                .ToListAsync();
        }
        //----------------------------------------------------------------------------------// 

        //This gets one job card using its ID. 
        public async Task<JobCard?> GetJobCardAsync(int id)
        {
            //This searches the database for the selected job card. 
            return await _context.JobCards
                .Include(j => j.Supplier)
                .FirstOrDefaultAsync(j => j.JobCardID == id);
        }
        //----------------------------------------------------------------------------------// 

        //This gets all suppliers so a supplier can be assigned to a Job Card. 
        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            return await _context.Suppliers
                .OrderBy(s => s.SupplierName)
                .ToListAsync();
        }
        //----------------------------------------------------------------------------------// 

        //This assigns a supplier to an existing Job Card. 
        public async Task<bool> AssignSupplierAsync(int id, int supplierId)
        {
            //This searches for the selected Job Card. 
            var jobCard = await _context.JobCards
                .FirstOrDefaultAsync(j => j.JobCardID == id);

            //This checks whether the Job Card exists. 
            if (jobCard == null)
            {
                return false;
            }

            //This searches for the selected Supplier. 
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierID == supplierId);

            //This checks whether the Supplier exists. 
            if (supplier == null)
            {
                return false;
            }

            //This assigns the Supplier to the Job Card. 
            jobCard.SupplierID = supplierId;

            //This saves the assignment. 
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("JobCardsUpdated");

            return true;
        }
        //----------------------------------------------------------------------------------// 

        //This creates a job card automatically from an accepted quote. 
        public async Task<(bool Success, string? Error)> CreateFromAcceptedQuoteAsync(int
quoteId)
        {
            //This gets the accepted quote together with its items. 
            var quote = await _context.Quotes
                .Include(q => q.QuoteItems)
                .FirstOrDefaultAsync(q => q.QuoteID == quoteId);

            //This checks whether the quote exists. 
            if (quote == null)
            {
                return (false, "Quote not found.");
            }

            //This checks that the quote has actually been accepted. 
            if (quote.QuoteStatus != "Accepted")
            {
                return (false, "Only an accepted quote can create a job card.");
            }

            //This creates a unique job card reference from the quote reference. 
            var jobReference = $"{quote.ReferenceNumber}-JOB";

            //This checks whether this accepted quote already created a job card. 
            var existingJobCard = await _context.JobCards
                .FirstOrDefaultAsync(j => j.ReferenceNumber == jobReference);

            //This prevents duplicate job cards from being created. 
            if (existingJobCard != null)
            {
                return (true, null);
            }

            //This gets the client linked to the quote. 
            var client = await _context.Clients.FindAsync(quote.ClientID);

            //This checks whether the client still exists. 
            if (client == null)
            {
                return (false, "The client linked to this quote could not be found.");
            }

            //This builds a readable job title using the client name. 
            var jobTitle = $"Job for {client.FirstName} {client.LastName}";

            //This builds the job description from the products on the quote. 
            var productNames = quote.QuoteItems
                .Select(i => $"Product ID {i.ProductID} x {i.Quantity}")
                .ToList();

            var jobDescription = productNames.Count > 0
                ? string.Join(", ", productNames)
                : quote.Notes;

            //This creates the new job card. 
            var jobCard = new JobCard
            {
                ReferenceNumber = jobReference,
                ClientID = quote.ClientID,
                JobTitle = jobTitle,
                JobDescription = jobDescription,
                JobStatus = "Not Started",
                PaymentStatus = "Pending Payment",
                JobDate = DateTime.Now
            };

            //This adds the new job card to the database context. 
            _context.JobCards.Add(jobCard);

            //This does not save yet. 
            //The QuoteService saves both the accepted quote and job card together. 
            return (true, null);
        }
        //----------------------------------------------------------------------------------// 

        //This updates an existing job card. 
        public async Task<bool> UpdateJobCardAsync(int id, JobCard jobCard)
        {
            //This checks that the URL ID matches the job card ID. 
            if (id != jobCard.JobCardID)
            {
                return false;
            }

            //This searches for the existing job card. 
            var existingJobCard = await _context.JobCards
                .FirstOrDefaultAsync(j => j.JobCardID == id);

            //This checks whether the job card exists. 
            if (existingJobCard == null)
            {
                return false;
            }

            //This updates the job card fields. 
            existingJobCard.ReferenceNumber = jobCard.ReferenceNumber;
            existingJobCard.ClientID = jobCard.ClientID;
            existingJobCard.JobTitle = jobCard.JobTitle;
            existingJobCard.JobDescription = jobCard.JobDescription;
            existingJobCard.JobStatus = jobCard.JobStatus;
            existingJobCard.DueDate = jobCard.DueDate;
            existingJobCard.CompletedAt = jobCard.CompletedAt;
            existingJobCard.PortfolioSubmissionID = jobCard.PortfolioSubmissionID;

            //////////// 
            //The supplier is assigned separately through AssignSupplierAsync. 
            //This stops the normal Job Card edit from accidentally changing the assigned Supplier. 
            //////////// 

            //This saves the updated job card. 
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("JobCardsUpdated");

            //This returns a successful result. 
            return true;
        }
        //----------------------------------------------------------------------------------// 

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var allowedStatuses = new[]
            {
                "Not Started",
                "In Progress",
                "Completed",
                "Archived"
            };

            if (!allowedStatuses.Contains(status))
            {
                return false;
            }

            var jobCard = await _context.JobCards
                .FirstOrDefaultAsync(j => j.JobCardID == id);

            if (jobCard == null)
            {
                return false;
            }

            jobCard.JobStatus = status;

            if (status == "Completed")
            {
                jobCard.CompletedAt = DateTime.Now;
            }
            else
            {
                jobCard.CompletedAt = null;
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("JobCardsUpdated");

            return true;
        }
        //----------------------------------------------------------------------------------// 

        //This deletes an existing job card. 
        public async Task<bool> DeleteJobCardAsync(int id)
        {
            //This searches for the job card. 
            var jobCard = await _context.JobCards
                .FirstOrDefaultAsync(j => j.JobCardID == id);

            //This checks whether the job card exists. 
            if (jobCard == null)
            {
                return false;
            }

            //This marks the job card for deletion. 
            _context.JobCards.Remove(jobCard);

            //This saves the deletion. 
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("JobCardsUpdated");

            return true;
        }
        //----------------------------------------------------------------------------------------------------// 

        public async Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus)
        {
            var allowedStatuses = new[] { "Pending Payment", "Paid" };

            if (!allowedStatuses.Contains(paymentStatus))
            {
                return false;
            }

            var jobCard = await _context.JobCards
                .FirstOrDefaultAsync(j => j.JobCardID == id);

            if (jobCard == null)
            {
                return false;
            }

            jobCard.PaymentStatus = paymentStatus;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("JobCardsUpdated");

            return true;
        }
    }
}
