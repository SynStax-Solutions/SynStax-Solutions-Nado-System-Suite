using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Backend.Models
{
    // EF Core entity mapped to dbo.ProductCategory. Simple grouping/lookup
    // table used to organize Products (e.g. for filtering in the UI).
    public class ProductCategory
    {
        //--------------------------------------------------------------------
        [Key]
        public int ProductCategoryID { get; set; }

        //--------------------------------------------------------------------
        // Enforced unique in ProductCategoryService.CreateCategoryAsync 
        // two categories can't share the same name.
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        //--------------------------------------------------------------------
        [MaxLength(255)]
        public string? CategoryDescription { get; set; }

        //--------------------------------------------------------------------
        public bool IsActive { get; set; } = true;
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------