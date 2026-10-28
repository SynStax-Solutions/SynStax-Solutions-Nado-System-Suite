using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;

namespace Nado_Graphics_MS_Backend.Services
{
    public class SupplierService
    {
        private readonly ISupplierRepository _repo;

        public SupplierService(ISupplierRepository repo) => _repo = repo;

        public Task<List<Supplier>> GetSuppliersAsync() => _repo.GetAllAsync();

        public Task<Supplier?> GetSupplierAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<bool> CreateSupplierAsync(Supplier supplier)
        {
            supplier.SupplierID = 0;
            supplier.CreatedAt = DateTime.Now;

            if (await _repo.NameExistsAsync(supplier.SupplierName)) return false;

            if (!string.IsNullOrWhiteSpace(supplier.SupplierEmail) &&
                await _repo.EmailExistsAsync(supplier.SupplierEmail))
                return false;

            await _repo.AddAsync(supplier);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSupplierAsync(int id, Supplier supplier)
        {
            if (id != supplier.SupplierID) return false;

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            if (await _repo.NameExistsAsync(supplier.SupplierName, id)) return false;

            if (!string.IsNullOrWhiteSpace(supplier.SupplierEmail) &&
                await _repo.EmailExistsAsync(supplier.SupplierEmail, id))
                return false;

            existing.SupplierName = supplier.SupplierName;
            existing.ContactNumber = supplier.ContactNumber;
            existing.SupplierEmail = supplier.SupplierEmail;
            existing.SupplierAddress = supplier.SupplierAddress;
            existing.LeadTime = supplier.LeadTime;
            existing.ServiceType = supplier.ServiceType;

            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            var supplier = await _repo.GetByIdAsync(id);
            if (supplier == null) return false;

            _repo.Remove(supplier);
            await _repo.SaveChangesAsync();
            return true;
        }
    }
}