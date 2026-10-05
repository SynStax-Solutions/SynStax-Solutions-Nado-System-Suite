namespace Nado_Graphics_MS_Frontend.Models
{
    public class PaymentModel
    {
        public int PaymentID { get; set; }
        public int InvoiceID { get; set; }
        public int ClientID { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;   // Cash / EFT / Card / Other
        public string TransactionReference { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
