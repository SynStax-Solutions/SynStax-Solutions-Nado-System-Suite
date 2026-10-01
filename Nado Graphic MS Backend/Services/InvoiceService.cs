using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Models.InvoiceStates;
using Nado_Graphics_MS_Backend.Services;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace Nado_Graphics_MS_Backend.Services
{
    // The InvoiceService class provides methods to manage invoices, including creating, updating, deleting, and recording payments for invoices.
    // It interacts with the database context and handles business logic related to invoice management.
    public class InvoiceService
    {
        private readonly BMSDbContext _context;
        private readonly ReferenceNumberService _referenceNumberService;

        // Valid invoice statuses that can be assigned to an invoice. This array is used to validate the status of an invoice before saving it to the database.
        private static readonly string[] ValidStatuses = { "Unpaid", "Paid", "Partially Paid", "Overdue", "Cancelled", "Extension Requested" };

        public InvoiceService(BMSDbContext context, ReferenceNumberService referenceNumberService)
        {
            _context = context;
            _referenceNumberService = referenceNumberService;
        }
        //------------------------------------------------------
        // The GetInvoicesAsync method retrieves a list of all invoices from the database, including their associated invoice items,
        // and orders them by the invoice date in descending order. This allows for easy access to the most recent invoices.
        public async Task<List<Invoice>> GetInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.InvoiceItems)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }
        //----------------------------------------------------
        // The GetInvoiceAsync method retrieves a specific invoice by its ID from the database, including its associated invoice items.
        // If the invoice is not found, it returns null. This method is useful for viewing or editing a specific invoice.
        public async Task<Invoice?> GetInvoiceAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.InvoiceID == id);
        }
        //----------------------------------------------------
        // The CreateInvoiceManuallyAsync method creates a new invoice in the database based on the provided invoice object.
        // It validates the invoice data, including the client, invoice status, and invoice items, before saving it to the database.
        // If the invoice is created successfully, it returns a success result; otherwise, it returns an error message.
        public async Task<(bool Success, string? Error)> CreateInvoiceManuallyAsync(Invoice invoice)
        {
            // Reset the invoice ID and set the invoice date to the current date and time
            // This ensures that a new invoice is created with a unique ID and the current date.
            invoice.InvoiceID = 0;
            invoice.InvoiceDate = DateTime.Now;

            // Validate the client associated with the invoice. If the client is not found, return an error message.
            var client = await _context.Clients.FindAsync(invoice.ClientID);
            if (client == null)
            {
                return (false, "Client not found.");
            }

            // Validate the invoice status. If the status is null or empty, set it to "Unpaid". If the status is not valid, return an error message.
            if (string.IsNullOrWhiteSpace(invoice.InvoiceStatus))
            {
                invoice.InvoiceStatus = "Unpaid";
            }

            // Validate the invoice status against the list of valid statuses. If the status is not valid, return an error message.
            if (!ValidStatuses.Contains(invoice.InvoiceStatus))
            {
                return (false, "Invalid invoice status.");
            }

            // Validate that the invoice has at least one item. If there are no items, return an error message.
            if (invoice.InvoiceItems == null || !invoice.InvoiceItems.Any())
            {
                return (false, "A invoice must have at least one item.");
            }

            decimal total = 0;

            // Iterate through each invoice item to validate the product, quantity, and price. Calculate the total amount for the invoice.
            // If any validation fails, return an error message.
            foreach (var item in invoice.InvoiceItems)
            {
                item.InvoiceItemID = 0;

                var product = await _context.Products.FindAsync(item.ProductID);
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
                total += item.LineTotal;
            }

            // Set the total amount and amount paid for the invoice, generate a reference number, and set the due date to 30 days from now.
            invoice.TotalAmount = total;
            invoice.AmountPaid = 0;
            invoice.ReferenceNumber = await _referenceNumberService.GenerateReferenceNumberAsync(client.FirstName);
            // Set the due date to 30 days from now
            int addDays = 30;
            DateTime duetime = DateTime.Now.AddDays(addDays);

            invoice.DueDate = duetime;

            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync();

            return (true, null);
        }

        //----------------------------------------------------
        // The CreateInvoiceFromQuoteAsync method creates a new invoice based on an existing quote. It retrieves the quote and its items from the database,
        // validates the quote and its items, and then creates a new invoice with the same items and total amount. If the invoice is created successfully,
        // it returns the created invoice; otherwise, it returns an error message.
        public async Task<(bool Success, string? Error, Invoice? Invoice)> CreateInvoiceFromQuoteAsync(int quoteId)
        {

            // Retrieve the quote and its items from the database. If the quote is not found, return an error message.
            var quote = await _context.Quotes
                .Include(q => q.QuoteItems)
                .FirstOrDefaultAsync(q => q.QuoteID == quoteId);
            if (quote == null) return (false, "Quote not found.", null);

            if (quote.QuoteStatus != States.QuoteStatus.Accepted.ToString())
                return (false, "Can only create an invoice from an accepted quote.", null);

            if (await _context.Invoices.AnyAsync(i => i.QuoteID == quoteId))
                return (false, "An invoice already exists for this quote.", null);

            if (quote.QuoteItems == null || !quote.QuoteItems.Any())
                return (false, "The quote has no items.", null);

            var client = await _context.Clients.FindAsync(quote.ClientID);
            if (client == null) return (false, "Client not found.", null);

            decimal total = 0;
            var items = new List<InvoiceItem>();

            // Iterate through each quote item to validate the product, quantity, and price. Calculate the total amount for the invoice.
            // If any validation fails, return an error message.
            foreach (var item in quote.QuoteItems)
            {
                var product = await _context.Products.FindAsync(item.ProductID);
                if (product == null || !product.IsActive)
                    return (false, $"Product {item.ProductID} not found or inactive.", null);
                if (item.Quantity <= 0)
                    return (false, "Quantity must be greater than zero.", null);

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
                InvoiceStatus = States.InvoiceStatus.Unpaid.ToString(),
                InvoiceItems = items,
                TotalAmount = total,
                AmountPaid = 0
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return (true, null, invoice);
        }

        //----------------------------------------------------
        // The UpdateInvoiceAsync method updates an existing invoice in the database based on the provided invoice object.
        // It validates the invoice data, including the client, invoice status, and invoice items, before saving the changes to the database.
        // If the invoice is updated successfully, it returns true; otherwise, it returns false.
        public async Task<bool> UpdateInvoiceAsync(int id, Invoice invoice)
        {
            // Validate that the provided ID matches the invoice ID. If they do not match, return false.
            if (id != invoice.InvoiceID)
            {
                return false;
            }

            // Validate that the invoice exists in the database. If it does not exist, return false.
            var existingInvoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.InvoiceID == id);

            // Validate that the invoice is not already paid or cancelled. If it is, return false.
            if (existingInvoice == null)
            {
                return false;
            }

            // Validate that the invoice is not already paid or cancelled. If it is, return false.
            if (existingInvoice.InvoiceStatus == States.InvoiceStatus.Paid.ToString() || existingInvoice.InvoiceStatus == States.InvoiceStatus.Cancelled.ToString())
            {
                return false;
            }

            // Validate the client associated with the invoice. If the client is not found, return false.
            var client = await _context.Clients.FindAsync(invoice.ClientID);
            if (client == null)
            {
                return false;
            }

            // Validate the invoice status. If the status is null or empty, set it to "Unpaid". If the status is not valid, return false.
            if (invoice.InvoiceItems == null || !invoice.InvoiceItems.Any())
            {
                return false;
            }

            // Validate the invoice status against the list of valid statuses. If the status is not valid, return false.
            decimal total = 0;
            var newItems = new List<InvoiceItem>();

            // Iterate through each invoice item to validate the product, quantity, and price. Calculate the total amount for the invoice.
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

            // Remove existing invoice items from the database and replace them with the new items. Update the existing invoice's properties with the new values.
            _context.invoiceItems.RemoveRange(existingInvoice.InvoiceItems);

            // Update the existing invoice's properties with the new values
            existingInvoice.ClientID = invoice.ClientID;
            existingInvoice.DueDate = invoice.DueDate;
            existingInvoice.Notes = invoice.Notes;
            existingInvoice.InvoiceItems = newItems;
            existingInvoice.TotalAmount = total;
            existingInvoice.AmountPaid = invoice.AmountPaid;
            existingInvoice.InvoiceStatus = invoice.InvoiceStatus;

            await _context.SaveChangesAsync();

            return true;
        }

        //----------------------------------------------------
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

        //----------------------------------------------------
        // The ExtensionInvoiceAsync method extends the due date of an overdue invoice by 30 days and updates its status accordingly.
        // It requires a reason for the extension and validates that the invoice is overdue before making the changes.
        // If the extension is successful, it returns a success result; otherwise, it returns an error message.
        public async Task<(bool Success, string? Error)> ExtensionInvoiceAsync(int id, string reason)
        {
            // Validate that the invoice exists in the database. If it does not exist, return an error message.
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return (false, "Invoice not found.");
            }

            if (id != invoice.InvoiceID)
            {
                return (false, "Invalid invoice ID.");
            }

            // Validate that the invoice is overdue. If it is not overdue, return an error message.
            if (invoice.InvoiceStatus != States.InvoiceStatus.Overdue.ToString())
            {
                return (false, "Only overdue invoices can be extended.");
            }
            if (string.IsNullOrWhiteSpace(reason))
                return (false, "A reason is required for an extension.");

            // Extend the due date by 30 days, update the invoice status based on the amount paid, and add a note with the reason for the extension.
            invoice.DueDate = invoice.DueDate.AddDays(30);
            invoice.InvoiceStatus = invoice.AmountPaid > 0 ? States.InvoiceStatus.PartiallyPaid.ToString() : States.InvoiceStatus.Unpaid.ToString();
            invoice.Notes = $"Extension granted: {reason}. New due date: {invoice.DueDate.ToShortDateString()}";
            await _context.SaveChangesAsync();
            return (true, null);
        }

        //----------------------------------------------------
        // The RecordPaymentAsync method records a payment for an invoice, updating the amount paid and the invoice status accordingly.
        // It validates the payment amount and ensures that it does not exceed the outstanding balance of the invoice.
        // If the payment is recorded successfully, it returns a success result; otherwise, it returns an error message.
        public async Task<(bool Success, string? Error)> RecordPaymentAsync(int id, RecordPaymentRequest request)
        {
            // Validate that the invoice exists in the database. If it does not exist, return an error message.
            var invoice = await _context.Invoices.FindAsync(id);

            // Validate that the payment amount is greater than zero. If it is not, return an error message.
            if (invoice == null) return (false, "Invoice was not found");
            if (request.Amount <= 0) return (false, "Payment amount must be greater than zero.");

            var newAmountPaid = invoice.AmountPaid + request.Amount;
            if (newAmountPaid > invoice.TotalAmount)
                return (false, "Payment exceeds the outstanding balance.");
            // Validate that the invoice is not already paid or cancelled. If it is, return an error message.
            try
            {
                var state = InvoiceStateFactory.GetState(invoice);   // throws if the transition is illegal
                if (newAmountPaid >= invoice.TotalAmount) state.Paid(invoice);
                else state.PartiallyPaid(invoice);

                // Update the amount paid and add a new payment record to the database. Save changes to the database.
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
//----------------------------------------------------END OF FILE--------------------------------------------------------