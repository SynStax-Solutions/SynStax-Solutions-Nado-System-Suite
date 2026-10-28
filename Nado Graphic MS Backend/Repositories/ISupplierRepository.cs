using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public interface ISupplierRepository : IRepository<Supplier>
    {
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        Task<List<Supplier>> GetAllOrderedByNameAsync();
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------