namespace Nado_Graphics_MS_Frontend.Models
{
    public class ProductCategoryModel
    {
        public int ProductCategoryID { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string? CategoryDescription { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
//-------------------------------------------------------------END OF FILE-----------------------------------------------------------------------