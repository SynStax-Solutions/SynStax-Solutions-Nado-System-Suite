using System.ComponentModel.DataAnnotations;

namespace Nado_Graphics_MS_Frontend.Models
{
    public class ProductModel
    {
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public int ProductCategoryID { get; set; }

        [Required(ErrorMessage = "Please enter a product name.")]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
//-------------------------------------------------------------END OF FILE-----------------------------------------------------------------------