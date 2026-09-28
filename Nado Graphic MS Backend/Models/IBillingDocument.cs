namespace Nado_Graphics_MS_Backend.Models
{
    //Shared contract between Quote and Invoice - the two document types that
    //carry a client-facing reference number and a billed amount.

    public interface IBillingDocument
    {
        //-------------------------------------
        string ReferenceNumber { get; set; }

        //-------------------------------------
        int ClientID { get; set; }

        //-------------------------------------
        decimal TotalAmount { get; set; }

        //-------------------------------------
        string? Notes { get; set; }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------