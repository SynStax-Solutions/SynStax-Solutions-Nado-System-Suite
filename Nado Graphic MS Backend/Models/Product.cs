using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nado_Graphics_MS_Backend.Models
{
    // EF Core entity mapped to dbo.Product. Represents anything that can be
    // added as a line item to a Quote (and an Invoice/Job too).

    public class Product
    {
        //--------------------------------------------------------------------
        [Key]
        public int ProductID { get; set; }

        //--------------------------------------------------------------------
        // FK to the category this product belongs to (e.g. "Printing").
        [Required]
        public int ProductCategoryID { get; set; }

        //--------------------------------------------------------------------
        [Required]
        [MaxLength(150)]
        public string ProductName { get; set; } = string.Empty;

        //--------------------------------------------------------------------
        [MaxLength(500)]
        public string? Description { get; set; }

        //--------------------------------------------------------------------
        // Current listed price. Stored as decimal(10,2) for accurate money.
        // This is the price QuoteService falls back to whenever a
        // QuoteItem's UnitPrice is left unspecified (<= 0).
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        //--------------------------------------------------------------------
        // Rather than hard-deleting a discontinued product,
        // products are deactivated instead. QuoteService explicitly checks
        // this flag and refuses to add inactive products to new quotes.
        public bool IsActive { get; set; } = true;

        //--------------------------------------------------------------------
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------