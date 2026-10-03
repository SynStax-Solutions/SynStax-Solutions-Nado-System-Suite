using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Models.InvoiceStates;

namespace Nado_Graphics_MS_Backend.Services
{
    // The InvoiceService class provides methods to manage invoices, including creating, updating, deleting, and recording payments for invoices.
    // It interacts with the database context and handles business logic related to invoice management.
    public class InvoiceService
    {
        private readonly BMSDbContext _context;
        private readonly ReferenceNumberService _referenceNumberService;


        public InvoiceService(BMSDbContext context, ReferenceNumberService referenceNumberService)
        {
            _context = context;
            _referenceNumberService = referenceNumberService;
        }


        //----------------------------------------------------------------------------------//
        // The GetInvoicesAsync method retrieves a list of all invoices from the database, including their associated invoice items,
        // and orders them by the invoice date in descending order. This allows for easy access to the most recent invoices.
        public async Task<List<Invoice>> GetInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.InvoiceItems)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }
        //----------------------------------------------------------------------------------//
        // The GetInvoiceAsync method retrieves a specific invoice by its ID from the database, including its associated invoice items.
        // If the invoice is not found, it returns null. This method is useful for viewing or editing a specific invoice.
        public async Task<Invoice?> GetInvoiceAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.InvoiceID == id);
        }
        //----------------------------------------------------------------------------------//
        //----------------------------------------------------
        // The CreateInvoiceManuallyAsync method creates a new invoice in the database based on the provided invoice object.
        // It validates the invoice data, including the client, invoice status, and invoice items, before saving it to the database.
        // If the invoice is created successfully, it returns a success result; otherwise, it returns an error message.
        public async Task<(bool Success, string? Error, Invoice? Invoice)> CreateInvoiceManuallyAsync(CreateManualInvoiceRequest request)
        {
            var client = await _context.Clients.FindAsync(request.ClientID);

            if (client == null)
            {
                return (false, "Client not found.", null);
            }

            if (request.InvoiceItems == null || !request.InvoiceItems.Any())
            {
                return (false, "An invoice must have at least one item.", null);
            }

            decimal total = 0;
            var items = new List<InvoiceItem>();

            foreach (var item in request.InvoiceItems)
            {
                var product = await _context.Products.FindAsync(item.ProductID);

                if (product == null || !product.IsActive)
                {
                    return (false, $"Product {item.ProductID} not found or inactive.", null);
                }

                if (item.Quantity <= 0)
                {
                    return (false, "Quantity must be greater than zero.", null);
                }

                var unitPrice = item.UnitPrice is > 0 ? item.UnitPrice.Value : product.Price;
                var lineTotal = item.Quantity * unitPrice;

                items.Add(new InvoiceItem
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal
                });

                total += lineTotal;
            }

            var invoice = new Invoice
            {
                QuoteID = null,
                ClientID = request.ClientID,
                InvoiceDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30),
                InvoiceStatus = States.InvoiceStatus.Unpaid.ToDbString(),
                ReferenceNumber = await _referenceNumberService.GenerateReferenceNumberAsync(client.FirstName),
                Notes = request.Notes,
                InvoiceItems = items,
                TotalAmount = total,
                AmountPaid = 0
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return (true, null, invoice);
        }
        //----------------------------------------------------------------------------------
        // The CreateInvoiceFromQuoteAsync method creates a new invoice based on an existing quote. It retrieves the quote and its items from the database,
        // validates the quote and its items, and then creates a new invoice with the same items and total amount. If the invoice is created successfully,
        // it returns the created invoice; otherwise, it returns an error message.
        public async Task<(bool Success, string? Error, Invoice? Invoice)> CreateInvoiceFromQuoteAsync(int quoteId)
        {
            var quote = await _context.Quotes
                .Include(q => q.QuoteItems)
                .FirstOrDefaultAsync(q => q.QuoteID == quoteId);

            if (quote == null)
            {
                return (false, "Quote not found.", null);
            }

            if (quote.QuoteStatus != States.QuoteStatus.Accepted.ToString())
            {
                return (false, "Can only create an invoice from an accepted quote.", null);
            }

            if (await _context.Invoices.AnyAsync(i => i.QuoteID == quoteId))
            {
                return (false, "An invoice already exists for this quote.", null);
            }

            if (quote.QuoteItems == null || !quote.QuoteItems.Any())
            {
                return (false, "The quote has no items.", null);
            }

            var client = await _context.Clients.FindAsync(quote.ClientID);

            if (client == null)
            {
                return (false, "Client not found.", null);
            }

            decimal total = 0;
            var items = new List<InvoiceItem>();

            // Iterate through each quote item to validate the product, quantity, and price. Calculate the total amount for the invoice.
            // If any validation fails, return an error message.
            foreach (var item in quote.QuoteItems)
            {
                var product = await _context.Products.FindAsync(item.ProductID);

                if (product == null || !product.IsActive)
                {
                    return (false, $"Product {item.ProductID} not found or inactive.", null);
                }

                if (item.Quantity <= 0)
                {
                    return (false, "Quantity must be greater than zero.", null);
                }

                var unitPrice = item.UnitPrice > 0 ? item.UnitPrice : product.Price;
                var lineTotal = item.Quantity * unitPrice;

                items.Add(new InvoiceItem
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal
                });

                total += lineTotal;
            }

            // Create a new invoice based on the quote and its items. Set the invoice date, due date, client ID, reference number,
            // invoice status, total amount, and amount paid. Add the invoice to the database and save changes.
            // Set the invoice date to now and the due date to 30 days from now
            var invoice = new Invoice
            {
                InvoiceDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30),
                QuoteID = quote.QuoteID,
                ClientID = quote.ClientID,
                ReferenceNumber = quote.ReferenceNumber,
                InvoiceStatus = States.InvoiceStatus.Unpaid.ToDbString(),
                InvoiceItems = items,
                TotalAmount = total,
                AmountPaid = 0
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return (true, null, invoice);
        }
        //----------------------------------------------------------------------------------//
        //Updates detail fields only - client, due date, notes, and line items (while still
        //Unpaid/PartiallyPaid/Overdue). Status is intentionally NOT touched here - it can
        //only change via RecordPaymentAsync, ExtensionInvoiceAsync, or CancelInvoiceAsync,
        //so the state pattern's transition rules can never be bypassed.
        public async Task<bool> UpdateInvoiceAsync(int id, Invoice invoice)
        {
            if (id != invoice.InvoiceID)
            {
                return false;
            }

            var existingInvoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.InvoiceID == id);

            if (existingInvoice == null)
            {
                return false;
            }

            if (existingInvoice.InvoiceStatus == States.InvoiceStatus.Paid.ToDbString()
                || existingInvoice.InvoiceStatus == States.InvoiceStatus.Cancelled.ToDbString())
            {
                return false;
            }

            var client = await _context.Clients.FindAsync(invoice.ClientID);

            if (client == null)
            {
                return false;
            }

            if (invoice.InvoiceItems == null || !invoice.InvoiceItems.Any())
            {
                return false;
            }

            decimal total = 0;
            var newItems = new List<InvoiceItem>();

            foreach (var item in invoice.InvoiceItems)
            {
                var product = await _context.Products.FindAsync(item.ProductID);

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

                newItems.Add(new InvoiceItem
                {
                    InvoiceID = existingInvoice.InvoiceID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal
                });

                total += lineTotal;
            }

            //An edited total can never end up lower than what's already been paid.
            if (existingInvoice.AmountPaid > total)
            {
                return false;
            }

            _context.InvoiceItems.RemoveRange(existingInvoice.InvoiceItems);

            existingInvoice.ClientID = invoice.ClientID;
            existingInvoice.DueDate = invoice.DueDate;
            existingInvoice.Notes = invoice.Notes;
            existingInvoice.InvoiceItems = newItems;
            existingInvoice.TotalAmount = total;

            await _context.SaveChangesAsync();

            return true;
        }
        //----------------------------------------------------------------------------------
        // The DeleteInvoiceAsync method deletes an existing invoice from the database based on the provided invoice ID.
        // It first checks if the invoice exists in the database. If it does, it removes the invoice and saves the changes to the database.
        // If the invoice is deleted successfully, it returns true; otherwise, it returns false.
        public async Task<bool> DeleteInvoiceAsync(int id)
        {
            // Validate that the invoice exists in the database. If it does not exist, return false.
            var invoice = await _context.Invoices.FindAsync(id);

            // Validate that the invoice is not already paid or cancelled. If it is, return false.
            if (invoice == null)
            {
                return false;
            }

            // Remove the invoice from the database and save changes.
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            return true;
        }
        //----------------------------------------------------------------------------------//
        public async Task<(bool Success, string? Error)> ExtensionInvoiceAsync(int id, string reason)
        {
            // Validate that the invoice exists in the database. If it does not exist, return an error message.
            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice == null)
            {
                return (false, "Invoice not found.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return (false, "A reason is required for an extension.");
            }

            try
            {
                var state = InvoiceStateFactory.GetState(invoice);
                state.ExtensionRequested(invoice, reason);

                invoice.DueDate = invoice.DueDate.AddDays(30);

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }
        }
        //----------------------------------------------------------------------------------//
        public async Task<(bool Success, string? Error)> CancelInvoiceAsync(int id, string reason)
        {
            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice == null)
            {
                return (false, "Invoice not found.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return (false, "A reason is required to cancel an invoice.");
            }

            try
            {
                var state = InvoiceStateFactory.GetState(invoice);
                state.Cancelled(invoice, reason);


                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }
        }
        //----------------------------------------------------------------------------------
        // The RecordPaymentAsync method records a payment for an invoice, updating the amount paid and the invoice status accordingly.
        // It validates the payment amount and ensures that it does not exceed the outstanding balance of the invoice.
        // If the payment is recorded successfully, it returns a success result; otherwise, it returns an error message.
        public async Task<(bool Success, string? Error)> RecordPaymentAsync(int id, RecordPaymentRequest request)
        {
            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice == null)
            {
                return (false, "Invoice was not found.");
            }

            if (request.Amount <= 0)
            {
                return (false, "Payment amount must be greater than zero.");
            }

            var newAmountPaid = invoice.AmountPaid + request.Amount;

            if (newAmountPaid > invoice.TotalAmount)
            {
                return (false, "Payment exceeds the outstanding balance.");
            }

            try
            {
                var state = InvoiceStateFactory.GetState(invoice);

                if (newAmountPaid >= invoice.TotalAmount)
                {
                    state.Paid(invoice);
                }
                else
                {
                    state.PartiallyPaid(invoice);
                }

                invoice.AmountPaid = newAmountPaid;

                _context.Payments.Add(new Payment
                {
                    InvoiceID = invoice.InvoiceID,
                    ClientID = invoice.ClientID,
                    ReferenceNumber = invoice.ReferenceNumber,
                    PaymentDate = DateTime.Now,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    TransactionReference = request.TransactionReference,
                    Notes = request.Notes
                });

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------//