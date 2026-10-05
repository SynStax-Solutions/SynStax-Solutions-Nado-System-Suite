using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Nado_Graphics_MS_Backend.Security;

namespace Nado_Graphics_MS_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    //Only a logged-in Manager (Owner) or PA/Admin can use these endpoints.
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class OrganisationsController : ControllerBase
    {
        //This stores the database context used to access organisation records.
        private readonly BMSDbContext _context;

        //This receives the database contextusing dependency Injection
        public OrganisationsController(BMSDbContext context)
        {
            _context = context;
        }
        //--------------------------------------------------------------------------------------------------------------//
        //This gets all organisations from the database
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organisation>>> GetOrganisations()
        {
            //This retrieves all organisation records.
            return await _context.Organisations.ToListAsync();
        }
        //--------------------------------------------------------------------------------------------------------------//

        //This gets one organisation using its ID.[HttpGet("{id}")]
        public async Task<ActionResult<Organisation>> GetOrganisation(int id)
        {
            //This searches for the organisation using the supplied ID.
            var organisation = await _context.Organisations.FindAsync(id);
            //This checks whether the organisation does not exist
            if (organisation == null)
            {
                //This returns a 404 response when the organisation is not found.
                return NotFound();
            }
            //This returns the organisation to the frontend.
            return organisation;
        }
        //--------------------------------------------------------------------------------------------------------------//
        //This creates a new organisation.
        [HttpPost]
        public async Task<ActionResult<Organisation>> CreateOrganisation(
            Organisation organisation)
        {
            //This checks whether another organisation already uses the same name.
            var nameExists = await _context.Organisations.AnyAsync(o => o.OrganisationName == organisation.OrganisationName);

            //This stops duplicate organisation names from being created.
            if (nameExists)
            {
                //This returns a conflict response when the name already exists.
                return Conflict("An organisation with this name already exists.");
            }

            //This resets the ID so the database generates a new identity value.
            organisation.OrganisationID = 0;
            //This records the date and time that the organisation is created.
            organisation.CreatedAt = DateTime.Now;

            //This adds the new organisation to the database context.
            _context.Organisations.Add(organisation);

            //This saves the new organisation to the database.
            await _context.SaveChangesAsync();

            //This returns the newly created organisation together with its generated ID.
            return CreatedAtAction(nameof(GetOrganisation), new { id = organisation.OrganisationID }, organisation);
        }
        //--------------------------------------------------------------------------------------------------------------//
        //This updates an existing organisation.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrganisation(int id, Organisation organisation)
        {
            //This checks that the ID in the URL matches the ID in the submitted organisation.
            if (id != organisation.OrganisationID)
            {
                //This returns a bad request when the IDs do not match.
                return BadRequest();
            }

            //This searches for the existing organisation using its ID.
            var existingOrganisation = await _context.Organisations.FindAsync(id);

            //This checks whether the organisation does not exist.
            if (existingOrganisation == null)
            {
                //This returns a 404 response when the organisation is not found.
                return NotFound();
            }

            //This checks whether another organisation already uses the submitted name.
            var nameExists = await _context.Organisations.AnyAsync(o => o.OrganisationName == organisation.OrganisationName &&
                    o.OrganisationID != id);

            //This stops two different organisations from using the same name.
            if (nameExists)
            {
                //This returns a conflict response when the organisation name already exists.
                return Conflict("An organisation with this name already exists.");
            }

            //This updates the organisation name.
            existingOrganisation.OrganisationName = organisation.OrganisationName;

            //This updates the registration number.
            existingOrganisation.RegistrationNumber = organisation.RegistrationNumber;

            //This updates the email address.
            existingOrganisation.Email = organisation.Email;

            //This updates the phone number.
            existingOrganisation.Phone = organisation.Phone;

            //This updates the billing address.
            existingOrganisation.BillingAddress = organisation.BillingAddress;

            //This saves all organisation changes to the database.
            await _context.SaveChangesAsync();

            //This returns a successful response without a response body.
            return NoContent();
        }
        //--------------------------------------------------------------------------------------------------------------//
        //This deletes an existing organisation.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganisation(int id)
        {
            //This searches for the organisation using its ID.
            var organisation = await _context.Organisations.FindAsync(id);

            //This checks whether the organisation does not exist.
            if (organisation == null)
            {
                //This returns a 404 response when the organisation is not found.
                return NotFound();
            }

            //This marks the organisation for deletion.
            _context.Organisations.Remove(organisation);

            //This permanently saves the deletion to the database.
            await _context.SaveChangesAsync();

            //This returns a successful response without a response body.
            return NoContent();
        }
    }
}
/* References
Microsoft (n.d.) Role-based authorization in ASP.NET Core. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles
 */
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//