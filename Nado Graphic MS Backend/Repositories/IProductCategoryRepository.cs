using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public interface IProductCategoryRepository : IRepository<ProductCategory>
    {
        Task<bool> NameExistsAsync(string name);
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------