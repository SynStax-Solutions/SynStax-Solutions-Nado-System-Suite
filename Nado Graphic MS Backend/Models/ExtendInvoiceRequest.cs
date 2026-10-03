using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models
{
    public class ExtendInvoiceRequest
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------