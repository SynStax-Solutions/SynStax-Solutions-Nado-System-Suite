using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganisationsController : ControllerBase
    {
        private readonly BMSDbContext _context;

        public OrganisationsController(BMSDbContext context)
        {
            _context = context;
        }
        //--------------------------------------------------------------------------------------------------------------//
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organisation>>> GetOrganisations()
        {
            return await _context.Organisations.ToListAsync();
        }
        //--------------------------------------------------------------------------------------------------------------//
        [HttpGet("{id}")]
        public async Task<ActionResult<Organisation>> GetOrganisation(int id)
        {
            var organisation = await _context.Organisations.FindAsync(id);

            if (organisation == null)
            {
                return NotFound();
            }

            return organisation;
        }
        //--------------------------------------------------------------------------------------------------------------//
        [HttpPost]
        public async Task<ActionResult<Organisation>> CreateOrganisation(
            Organisation organisation)
        {
            var nameExists = await _context.Organisations.AnyAsync(o => o.OrganisationName == organisation.OrganisationName);

            if (nameExists)
            {
                return Conflict("An organisation with this name already exists.");
            }

            organisation.OrganisationID = 0;
            organisation.CreatedAt = DateTime.Now;

            _context.Organisations.Add(organisation);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrganisation), new { id = organisation.OrganisationID }, organisation);
        }
        //--------------------------------------------------------------------------------------------------------------//
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrganisation(
            int id,
            Organisation organisation)
        {
            if (id != organisation.OrganisationID)
            {
                return BadRequest();
            }

            var existingOrganisation = await _context.Organisations.FindAsync(id);

            if (existingOrganisation == null)
            {
                return NotFound();
            }

            var nameExists = await _context.Organisations.AnyAsync(o => o.OrganisationName == organisation.OrganisationName &&
                    o.OrganisationID != id);

            if (nameExists)
            {
                return Conflict("An organisation with this name already exists.");
            }

            existingOrganisation.OrganisationName = organisation.OrganisationName;

            existingOrganisation.RegistrationNumber = organisation.RegistrationNumber;

            existingOrganisation.Email = organisation.Email;

            existingOrganisation.Phone = organisation.Phone;

            existingOrganisation.BillingAddress = organisation.BillingAddress;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        //--------------------------------------------------------------------------------------------------------------//
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganisation(int id)
        {
            var organisation = await _context.Organisations.FindAsync(id);

            if (organisation == null)
            {
                return NotFound();
            }

            _context.Organisations.Remove(organisation);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//