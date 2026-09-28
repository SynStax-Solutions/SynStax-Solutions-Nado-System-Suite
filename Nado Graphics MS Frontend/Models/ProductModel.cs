namespace Nado_Graphics_MS_Frontend.Models
{
    public class ProductModel
    {
        public int ProductID { get; set; }

        public int ProductCategoryID { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
//-------------------------------------------------------------END OF FILE-----------------------------------------------------------------------