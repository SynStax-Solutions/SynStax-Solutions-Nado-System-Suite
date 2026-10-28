using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Hubs;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Models.QuoteStates;
using Nado_Graphics_MS_Backend.Repositories;

namespace Nado_Graphics_MS_Backend.Services
{
    //This class is the service that handles quote business logic.
    // This is where all validation, total
    // calculation, and state transitions occur.
    public class QuoteService
    {
        private readonly IQuoteRepository _quotes;
        private readonly IRepository<Client> _clients;
        private readonly IRepository<Product> _products;
        private readonly ReferenceNumberService _referenceNumberService;
        //added the below service declaration
        private readonly JobCardService _jobCardService;
        private readonly IHubContext<JobCardHub> _hubContext;
        //The 4 statuses allowed by the Check_Quote_Status constraint.
        //was: { "Pending", "Accepted", "Rejected", "Expired" }
        private static readonly string[] ValidStatuses = Enum.GetNames<States.QuoteStatus>();

        public QuoteService(
            IQuoteRepository quotes,
            IRepository<Client> clients,
            IRepository<Product> products,
            ReferenceNumberService referenceNumberService,
            JobCardService jobCardService,
            IHubContext<JobCardHub> hubContext)
        {
            _quotes = quotes;
            _clients = clients;
            _products = products;
            _referenceNumberService = referenceNumberService;
            _jobCardService = jobCardService;
            _hubContext = hubContext;
        }

        //----------------------------------------------------------------------------------
        // Returns every quote, newest first, with their line items
        // loaded so the API response contains full quote details
        // in a single query
        public async Task<List<Quote>> GetQuotesAsync()
        {
            return await _quotes.GetAllWithItemsAsync();                
        }
        //----------------------------------------------------------------------------------
        // Fetches a single quote with its items. Returns null if not found
        // the controller turns that into a 404.
        public async Task<Quote?> GetQuoteAsync(int id)
        {
            return await _quotes.GetWithItemsAsync(id);                
        }
        //----------------------------------------------------------------------------------
        // Creates a new quote from scratch, including validating and pricing
        // every line item, then calculating the total and generating a
        // reference number.
        public async Task<(bool Success, string? Error)> CreateQuoteAsync(Quote quote)
        {
            // Force these to values regardless of what the caller
            // sent, QuoteID must be 0 for EF to treat this as an insert, and
            // QuoteDate should always be "now", not whatever a client passed.
            quote.QuoteID = 0;
            quote.QuoteDate = DateTime.Now;

            // A quote can't exist without a client linked to it.
            var client = await _clients.GetByIdAsync(quote.ClientID);

            // If no status was supplied, default to Pending - this is the
            // only valid starting state for a new quote.
            if (client == null)
            {
                return (false, "Client not found.");
            }

            if (string.IsNullOrWhiteSpace(quote.QuoteStatus))
            {
                quote.QuoteStatus = States.QuoteStatus.Pending.ToString();
            }

            if (!ValidStatuses.Contains(quote.QuoteStatus))
            {
                return (false, "Invalid quote status.");
            }

            //A quote must have at least one product/service on it.
            if (quote.QuoteItems == null || !quote.QuoteItems.Any())
            {
                return (false, "A quote must have at least one item.");
            }

            decimal total = 0;

            // Validate and price every line item.
            foreach (var item in quote.QuoteItems)
            {
                item.QuoteItemID = 0;

                var product = await _products.GetByIdAsync(item.ProductID);

                if (product == null || !product.IsActive)
                {
                    return (false, $"Product {item.ProductID} not found or inactive.");
                }

                if (item.Quantity <= 0)
                {
                    return (false, "Quantity must be greater than zero.");
                }

                //If the caller didn't specify a price, snapshot the product's current price.
                if (item.UnitPrice <= 0)
                {
                    item.UnitPrice = product.Price;
                }
                item.LineTotal = item.Quantity * item.UnitPrice;

                item.LineTotal = item.Quantity * item.UnitPrice;
                total += item.LineTotal;
            }

            //TotalAmount is now calculated from the line items - it's never trusted from the request body.
            quote.TotalAmount = total;

            quote.ReferenceNumber = await _referenceNumberService.GenerateReferenceNumberAsync(client.FirstName);

            await _quotes.AddAsync(quote);

            await _quotes.SaveChangesAsync();

            return (true, null);
        }
        //----------------------------------------------------------------------------------//
        // Updates an existing Pending quote: replaces its line items
        // entirely, recalculates the total, and updates client/notes/valid until. 
        public async Task<bool> UpdateQuoteAsync(int id, Quote quote)
        {
            if (id != quote.QuoteID)
            {
                return false;
            }

            var existingQuote = await _quotes.GetWithItemsAsync(id);                

            if (existingQuote == null)
            {
                return false;
            }

            //Only a Pending quote can be edited, once a client has responded, the
            //products/pricing underneath their decision shouldn't silently change.
            if (existingQuote.QuoteStatus != States.QuoteStatus.Pending.ToString())
            {
                return false;
            }

            var client = await _clients.GetByIdAsync(quote.ClientID);

            if (client == null)
            {
                return false;
            }

            //A quote must still have at least one item after editing.
            if (quote.QuoteItems == null || !quote.QuoteItems.Any())
            {
                return false;
            }

            decimal total = 0;
            var newItems = new List<QuoteItem>();

            // Build a fresh set of QuoteItem entities from the incoming data,
            // validating each one like CreateQuoteAsync.
            foreach (var item in quote.QuoteItems)
            {
                var product = await _products.GetByIdAsync(item.ProductID);

                if (product == null || !product.IsActive)
                {
                    return false;
                }

                if (item.Quantity <= 0)
                {
                    return false;
                }

                var unitPrice = item.UnitPrice > 0 ? item.UnitPrice : product.Price;
                var lineTotal = item.Quantity * unitPrice;

                newItems.Add(new QuoteItem
                {
                    QuoteID = existingQuote.QuoteID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal,
                    IsVatIncluded = item.IsVatIncluded
                });

                total += lineTotal;
            }

            //Removes the old line items so they can be fully replaced by the incoming set.
            _quotes.RemoveItems(existingQuote.QuoteItems);

            existingQuote.ClientID = quote.ClientID;
            existingQuote.ValidUntil = quote.ValidUntil;
            existingQuote.Notes = quote.Notes;
            existingQuote.QuoteItems = newItems;
            existingQuote.TotalAmount = total;

            //QuoteStatus is intentionally NOT updated here, it can only change via
            //AcceptQuoteAsync / RejectQuoteAsync / ExpireQuoteAsync.

            await _quotes.SaveChangesAsync();

            return true;
        }
        //----------------------------------------------------------------------------------//
        // Deletes a quote
        public async Task<bool> DeleteQuoteAsync(int id)
        {
            var quote = await _quotes.GetByIdAsync(id);

            if (quote == null)
            {
                return false;
            }

            _quotes.Remove(quote);

            await _quotes.SaveChangesAsync();

            return true;
        }
        //----------------------------------------------------------------------------------//
        //Attempts to move a quote to Accepted using the state pattern.
        public async Task<(bool Success, string? Error)> AcceptQuoteAsync(int id)
        {
            //This searches the database for the quote.
            var quote = await _quotes.GetByIdAsync(id);


            //This checks whether the quote exists.
            if (quote == null)
            {
                return (false, "Quote not found.");
            }

            try
            {
                //This gets the current quote state.
                var currentState = QuoteStateFactory.GetState(quote);

                //This changes the quote to Accepted.
                currentState.Accept(quote);

                //This creates the Job Card from the accepted quote.
                var jobCardResult = await _jobCardService.CreateFromAcceptedQuoteAsync(id);

                //This checks whether the Job Card could not be created.
                if (!jobCardResult.Success)
                {
                    return (false, jobCardResult.Error);
                }

                //This saves both the accepted quote and the new Job Card.
                await _quotes.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("JobCardsUpdated");
                //This returns a successful result.
                return (true, null);
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }

        }

        //public async Task<(bool Success, string? Error)> AcceptQuoteAsync(int id)
        //{
        //    var quote = await _context.Quotes.FindAsync(id);

        //    if (quote == null)
        //    {
        //        return (false, "Quote not found.");
        //    }

        //    try
        //    {
        //        var currentState = QuoteStateFactory.GetState(quote);

        //        currentState.Accept(quote);

        //        await _context.SaveChangesAsync();

        //        return (true, null);
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return (false, ex.Message);
        //    }
        //}
        //----------------------------------------------------------------------------------//
        // Same pattern as AcceptQuoteAsync, but for the Rejected transition.
        public async Task<(bool Success, string? Error)> RejectQuoteAsync(int id, string reason)
        {
            var quote = await _quotes.GetByIdAsync(id);

            if (quote == null)
            {
                return (false, "Quote not found.");
            }

            try
            {
                var currentState = QuoteStateFactory.GetState(quote);

                currentState.Reject(quote, reason);

                await _quotes.SaveChangesAsync();

                return (true, null);
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }
        }
        //----------------------------------------------------------------------------------//
        // Same pattern as AcceptQuoteAsync, but for the Expiry transition.
        public async Task<(bool Success, string? Error)> ExpireQuoteAsync(int id, string reason)
        {
            var quote = await _quotes.GetByIdAsync(id);

            if (quote == null)
            {
                return (false, "Quote not found.");
            }

            try
            {
                var currentState = QuoteStateFactory.GetState(quote);

                currentState.Expire(quote, reason);

                await _quotes.SaveChangesAsync();

                return (true, null);
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------