using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;

namespace Nado_Graphics_MS_Backend.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly BMSDbContext _context;
        protected readonly DbSet<T> _set;

        //-------------------------------------------------------------------------------
        public Repository(BMSDbContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }
        //-------------------------------------------------------------------------------
        public Task<List<T>> GetAllAsync() => _set.ToListAsync();
        //-------------------------------------------------------------------------------
        public async Task<T?> GetByIdAsync(int id) => await _set.FindAsync(id);
        //-------------------------------------------------------------------------------
        public async Task AddAsync(T entity) => await _set.AddAsync(entity);
        //-------------------------------------------------------------------------------
        public void Remove(T entity) => _set.Remove(entity);
        //-------------------------------------------------------------------------------
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
//-------------------------------------------------END OF FILE-----------------------------------------------------