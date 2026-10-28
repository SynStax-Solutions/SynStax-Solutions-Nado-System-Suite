using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(BMSDbContext context) : base(context) { }
        //-------------------------------------------------------------------------------
        public Task<Dictionary<int, string>> GetNamesByIdsAsync(List<int> productIds) =>
            _set.Where(p => productIds.Contains(p.ProductID))
                .ToDictionaryAsync(p => p.ProductID, p => p.ProductName);
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------