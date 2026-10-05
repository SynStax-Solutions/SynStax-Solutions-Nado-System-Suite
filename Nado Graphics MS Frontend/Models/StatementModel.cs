using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    // Mirrors the saved Statement record returned by the API.
    public class StatementModel
    {
        public int StatementID { get; set; }
        public int? ClientID { get; set; }
        public int? OrganisationID { get; set; }
        public string StatementReference { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DateGenerated { get; set; }
    }

    // One row on a statement: an invoice (debit) or a payment (credit) with the running balance.
    public class StatementLineModel
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }

    // The statement plus its calculated lines and totals (GET api/Statements/{id}).
    public class StatementDetailsModel
    {
        public StatementModel Statement { get; set; } = new();
        public string AccountName { get; set; } = string.Empty;
        public List<string> AddressLines { get; set; } = new();
        public decimal OpeningBalance { get; set; }
        public decimal TotalInvoiced { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<StatementLineModel> Lines { get; set; } = new();
    }

    // What the Create form binds to.
    public class CreateStatementViewModel
    {
        // "Client" or "Organisation"
        [Required]
        public string StatementFor { get; set; } = "Client";

        public int? ClientID { get; set; }

        public int? OrganisationID { get; set; }

        [Required(ErrorMessage = "Please choose a start date.")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Please choose an end date.")]
        public DateTime? EndDate { get; set; }
    }

    // What is actually sent to the API.
    public class CreateStatementRequestModel
    {
        public int? ClientID { get; set; }
        public int? OrganisationID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}