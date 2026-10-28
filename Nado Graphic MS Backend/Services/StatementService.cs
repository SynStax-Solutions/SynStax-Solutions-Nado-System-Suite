using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Models.InvoiceStates;
using Nado_Graphics_MS_Backend.Repositories;

namespace Nado_Graphics_MS_Backend.Services
{
    // Creates statement records and calculates their contents from invoices (debits) and payments (credits).
    // Cancelled invoices are ignored.
    public class StatementService
    {
        private readonly IStatementRepository _statements;
        private readonly IClientRepository _clients;
        private readonly IOrganisationRepository _organisations;
        private readonly IInvoiceRepository _invoices;

        public StatementService(IStatementRepository statements,
            IClientRepository clients,
            IOrganisationRepository organisations,
            IInvoiceRepository invoices)
        {
            _statements = statements;
            _clients = clients;
            _organisations = organisations;
            _invoices = invoices;
        }
        //----------------------------------------------------------------------------------
        public async Task<List<Statement>> GetStatementsAsync()
        {
            return await _statements.GetNewestFirstAsync();
        }
        //----------------------------------------------------------------------------------
        public async Task<Statement?> GetStatementAsync(int id)
        {
            return await _statements.GetByIdAsync(id);
        }
        //----------------------------------------------------------------------------------
        // Returns the statement together with its calculated lines and totals.
        public async Task<StatementData?> GetStatementDataAsync(int id)
        {
            var statement = await _statements.GetByIdAsync(id);

            if (statement == null)
            {
                return null;
            }

            var (data, _) = await BuildStatementDataAsync(statement);
            return data;
        }
        //----------------------------------------------------------------------------------
        public async Task<(bool Success, string? Error, Statement? Statement)> CreateStatementAsync(CreateStatementRequest request)
        {
            if ((request.ClientID == null) == (request.OrganisationID == null))
            {
                return (false, "Provide either a ClientID or an OrganisationID, not both.", null);
            }

            if (request.StartDate.Date > request.EndDate.Date)
            {
                return (false, "The start date must be on or before the end date.", null);
            }

            var statement = new Statement
            {
                ClientID = request.ClientID,
                OrganisationID = request.OrganisationID,
                StartDate = request.StartDate.Date,
                EndDate = request.EndDate.Date,
                DateGenerated = DateTime.Now,
                StatementReference = $"STM-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}"
            };

            // Build it first so we can validate the client/organisation and check there is something to show.
            var (data, error) = await BuildStatementDataAsync(statement);

            if (data == null)
            {
                return (false, error, null);
            }

            if (!data.Lines.Any() && data.OpeningBalance == 0)
            {
                return (false, "There is no invoice or payment activity in the selected period.", null);
            }

            await _statements.AddAsync(statement);
            await _statements.SaveChangesAsync();

            return (true, null, statement);
        }
        //----------------------------------------------------------------------------------
        public async Task<bool> DeleteStatementAsync(int id)
        {
            var statement = await _statements.GetByIdAsync(id);

            if (statement == null)
            {
                return false;
            }

            _statements.Remove(statement);
            await _statements.SaveChangesAsync();

            return true;
        }
        //----------------------------------------------------------------------------------
        // Works out the opening balance, the invoice/payment lines inside the period, and the totals.
        public async Task<(StatementData? Data, string? Error)> BuildStatementDataAsync(Statement statement)
        {
            var addressLines = new List<string>();
            string accountName;
            List<int> clientIds;

            if (statement.ClientID != null)
            {
                var client = await _clients.GetByIdAsync(statement.ClientID.Value);

                if (client == null)
                {
                    return (null, "Client not found.");
                }

                accountName = $"{client.FirstName} {client.LastName}";
                clientIds = new List<int> { client.ClientID };

                if (!string.IsNullOrWhiteSpace(client.OrganisationName)) addressLines.Add(client.OrganisationName);
                if (!string.IsNullOrWhiteSpace(client.StreetAddress)) addressLines.Add(client.StreetAddress);

                var cityLine = string.Join(" ", new[] { client.City, client.PostalCode }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
                if (!string.IsNullOrWhiteSpace(cityLine)) addressLines.Add(cityLine);
            }
            else
            {
                var organisation = statement.OrganisationID.HasValue
                    ? await _organisations.GetByIdAsync(statement.OrganisationID.Value)
                    : null;

                if (organisation == null)
                {
                    return (null, "Organisation not found.");
                }

                accountName = organisation.OrganisationName;

                clientIds = await _clients.GetIdsByOrganisationAsync(organisation.OrganisationID);

                if (!clientIds.Any())
                {
                    return (null, "This organisation has no clients.");
                }

                if (!string.IsNullOrWhiteSpace(organisation.BillingAddress)) addressLines.Add(organisation.BillingAddress);
                if (!string.IsNullOrWhiteSpace(organisation.Email)) addressLines.Add(organisation.Email);
            }

            var clientNames = await _clients.GetFullNamesByIdsAsync(clientIds);

            var start = statement.StartDate.Date;
            var endExclusive = statement.EndDate.Date.AddDays(1); // end date is inclusive
            var cancelled = States.InvoiceStatus.Cancelled.ToDbString();

            // Every non-cancelled invoice up to the end of the period (earlier ones feed the opening balance).
            var invoices = await _invoices.GetForStatementAsync(clientIds, cancelled, endExclusive);

            var invoiceIds = invoices.Select(i => i.InvoiceID).ToList();

            var payments = await _invoices.GetPaymentsForInvoicesAsync(invoiceIds, endExclusive);


            var opening = invoices.Where(i => i.InvoiceDate < start).Sum(i => i.TotalAmount)
                        - payments.Where(p => p.PaymentDate < start).Sum(p => p.Amount);

            var isOrganisation = statement.OrganisationID != null;
            var invoiceLookup = invoices.ToDictionary(i => i.InvoiceID);
            var lines = new List<StatementLineDto>();

            foreach (var invoice in invoices.Where(i => i.InvoiceDate >= start))
            {
                lines.Add(new StatementLineDto
                {
                    Date = invoice.InvoiceDate,
                    Type = "Invoice",
                    Reference = invoice.ReferenceNumber,
                    Description = isOrganisation
                        ? $"Invoice - {clientNames.GetValueOrDefault(invoice.ClientID)}"
                        : "Invoice",
                    Debit = invoice.TotalAmount
                });
            }

            foreach (var payment in payments.Where(p => p.PaymentDate >= start))
            {
                var method = payment.PaymentMethod.ToString();

                lines.Add(new StatementLineDto
                {
                    Date = payment.PaymentDate,
                    Type = "Payment",
                    Reference = invoiceLookup[payment.InvoiceID].ReferenceNumber,
                    Description = isOrganisation
                        ? $"Payment ({method}) - {clientNames.GetValueOrDefault(payment.ClientID)}"
                        : $"Payment ({method})",
                    Credit = payment.Amount
                });
            }

            // Chronological; an invoice is listed before a payment made at the same moment.
            lines = lines
                .OrderBy(l => l.Date)
                .ThenBy(l => l.Type == "Invoice" ? 0 : 1)
                .ToList();

            var running = opening;
            foreach (var line in lines)
            {
                running += line.Debit - line.Credit;
                line.Balance = running;
            }

            var totalInvoiced = lines.Sum(l => l.Debit);
            var totalPaid = lines.Sum(l => l.Credit);

            return (new StatementData
            {
                Statement = statement,
                AccountName = accountName,
                AddressLines = addressLines,
                OpeningBalance = opening,
                TotalInvoiced = totalInvoiced,
                TotalPaid = totalPaid,
                ClosingBalance = opening + totalInvoiced - totalPaid,
                Lines = lines
            }, null);
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------