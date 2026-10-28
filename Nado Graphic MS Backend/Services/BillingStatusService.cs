using Nado_Graphics_MS_Backend.Models.InvoiceStates;
using Nado_Graphics_MS_Backend.Models.QuoteStates;
using Nado_Graphics_MS_Backend.Repositories;

namespace Nado_Graphics_MS_Backend.Services
{
    //Moves invoices to Overdue and quotes to Expired when their dates pass.
    //The State pattern decides whether each transition is allowed.
    public class BillingStatusService
    {
        //Invoice.Notes and Quote.Notes are limited to 500 characters in the database.
        private const int MaxNotesLength = 500;

        private readonly IInvoiceRepository _invoices;
        private readonly IQuoteRepository _quotes;

        public BillingStatusService(IInvoiceRepository invoices, IQuoteRepository quotes)
        {
            _invoices = invoices;
            _quotes = quotes;
        }

        //Marks every invoice that is past its due date as Overdue. Returns how many changed.
        public async Task<int> MarkOverdueInvoicesAsync(DateTime? now = null)
        {
            var today = (now ?? DateTime.Now).Date;
            var candidates = await _invoices.GetOverdueCandidatesAsync(today);

            var changed = 0;
            foreach (var invoice in candidates)
            {
                try
                {
                    var reason = BuildReason(
                        $"Automatically marked overdue: payment was due on {invoice.DueDate:dd MMM yyyy}.",
                        invoice.Notes);

                    //The current state decides whether Overdue is allowed from here.
                    InvoiceStateFactory.GetState(invoice).Overdue(invoice, reason);
                    changed++;
                }
                catch (InvalidOperationException)
                {
                    //A transition the state does not allow (or an unknown status): skip this invoice.
                }
            }

            if (changed > 0) await _invoices.SaveChangesAsync();
            return changed;
        }

        //Expires every pending quote whose ValidUntil date has passed. Returns how many changed.
        public async Task<int> ExpireOutdatedQuotesAsync(DateTime? now = null)
        {
            var today = (now ?? DateTime.Now).Date;
            var candidates = await _quotes.GetExpiredPendingAsync(today);

            var changed = 0;
            foreach (var quote in candidates)
            {
                try
                {
                    var reason = BuildReason(
                        $"Automatically expired: the quote was valid until {quote.ValidUntil:dd MMM yyyy}.",
                        quote.Notes);

                    QuoteStateFactory.GetState(quote).Expire(quote, reason);
                    changed++;
                }
                catch (InvalidOperationException)
                {
                    //Not allowed from the current state: skip.
                }
            }

            if (changed > 0) await _quotes.SaveChangesAsync();
            return changed;
        }

        //The state classes store the reason in Notes, so keep the previous note inside it
        //and stay within the 500 character column limit.
        private static string BuildReason(string automaticNote, string? previousNote)
        {
            var reason = string.IsNullOrWhiteSpace(previousNote)
                ? automaticNote
                : $"{automaticNote} Previous note: {previousNote.Trim()}";

            return reason.Length <= MaxNotesLength
                ? reason
                : reason[..(MaxNotesLength - 3)] + "...";
        }
    }
}