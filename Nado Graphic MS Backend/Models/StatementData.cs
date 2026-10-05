namespace Nado_Graphics_MS_Backend.Models
{
    // Body of POST api/statements. Send EITHER ClientID OR OrganisationID.
    public class CreateStatementRequest
    {
        public int? ClientID { get; set; }
        public int? OrganisationID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class StatementLineDto
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;       // "Invoice" or "Payment"
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }                   // running balance
    }

    //Deliberately holds plain strings, not Client/Organisation
    // entities, so nothing sensitive (e.g. PasswordHash) is ever returned.
    public class StatementData
    {
        public Statement Statement { get; set; } = null!;
        public string AccountName { get; set; } = string.Empty;
        public List<string> AddressLines { get; set; } = new();
        public decimal OpeningBalance { get; set; }
        public decimal TotalInvoiced { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<StatementLineDto> Lines { get; set; } = new();
    }
}