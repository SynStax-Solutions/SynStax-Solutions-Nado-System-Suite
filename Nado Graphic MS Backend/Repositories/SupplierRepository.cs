using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public class SupplierRepository : Repository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(BMSDbContext context) : base(context) { }

        //-------------------------------------------------------------------------------
        public Task<bool> NameExistsAsync(string name, int? excludeId = null) =>
            _set.AnyAsync(s => s.SupplierName == name && (excludeId == null || s.SupplierID != excludeId));

        //-------------------------------------------------------------------------------
        public Task<bool> EmailExistsAsync(string email, int? excludeId = null) =>
            _set.AnyAsync(s => s.SupplierEmail == email && (excludeId == null || s.SupplierID != excludeId));
        //-------------------------------------------------------------------------------
        public Task<List<Supplier>> GetAllOrderedByNameAsync() =>
    _set.OrderBy(s => s.SupplierName).ToListAsync();
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------