using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        //ProductID to ProductName for the given products.
        Task<Dictionary<int, string>> GetNamesByIdsAsync(List<int> productIds);
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------