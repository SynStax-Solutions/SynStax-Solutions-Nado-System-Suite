using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public class ProductCategoryRepository : Repository<ProductCategory>, IProductCategoryRepository
    {
        //-------------------------------------------------------------------------------
        public ProductCategoryRepository(BMSDbContext context) : base(context) { }

        //-------------------------------------------------------------------------------
        public Task<bool> NameExistsAsync(string name) =>
            _set.AnyAsync(c => c.CategoryName == name);
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------