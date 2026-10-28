using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;

namespace Nado_Graphics_MS_Backend.Services
{
    public enum OrganisationOutcome { Ok, NotFound, IdMismatch, DuplicateName }

    public class OrganisationService
    {
        private readonly IOrganisationRepository _repo;

        public OrganisationService(IOrganisationRepository repo) => _repo = repo;

        public Task<List<Organisation>> GetOrganisationsAsync() => _repo.GetAllAsync();

        public Task<Organisation?> GetOrganisationAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<OrganisationOutcome> CreateOrganisationAsync(Organisation organisation)
        {
            if (await _repo.NameExistsAsync(organisation.OrganisationName))
                return OrganisationOutcome.DuplicateName;

            organisation.OrganisationID = 0;
            organisation.CreatedAt = DateTime.Now;

            await _repo.AddAsync(organisation);
            await _repo.SaveChangesAsync();
            return OrganisationOutcome.Ok;
        }

        public async Task<OrganisationOutcome> UpdateOrganisationAsync(int id, Organisation organisation)
        {
            if (id != organisation.OrganisationID) return OrganisationOutcome.IdMismatch;

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return OrganisationOutcome.NotFound;

            if (await _repo.NameExistsAsync(organisation.OrganisationName, id))
                return OrganisationOutcome.DuplicateName;

            existing.OrganisationName = organisation.OrganisationName;
            existing.RegistrationNumber = organisation.RegistrationNumber;
            existing.Email = organisation.Email;
            existing.Phone = organisation.Phone;
            existing.BillingAddress = organisation.BillingAddress;

            await _repo.SaveChangesAsync();
            return OrganisationOutcome.Ok;
        }

        public async Task<bool> DeleteOrganisationAsync(int id)
        {
            var organisation = await _repo.GetByIdAsync(id);
            if (organisation == null) return false;

            _repo.Remove(organisation);
            await _repo.SaveChangesAsync();
            return true;
        }
    }
}