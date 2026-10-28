using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Security;
using Nado_Graphics_MS_Backend.Services;

namespace Nado_Graphics_MS_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class OrganisationsController : ControllerBase
    {
        private readonly OrganisationService _organisationService;

        public OrganisationsController(OrganisationService organisationService)
        {
            _organisationService = organisationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organisation>>> GetOrganisations()
        {
            return Ok(await _organisationService.GetOrganisationsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Organisation>> GetOrganisation(int id)
        {
            var organisation = await _organisationService.GetOrganisationAsync(id);
            if (organisation == null) return NotFound();
            return Ok(organisation);
        }

        [HttpPost]
        public async Task<ActionResult<Organisation>> CreateOrganisation(Organisation organisation)
        {
            var outcome = await _organisationService.CreateOrganisationAsync(organisation);

            if (outcome == OrganisationOutcome.DuplicateName)
                return this.ApiConflict("An organisation with this name already exists.");

            return CreatedAtAction(nameof(GetOrganisation), new { id = organisation.OrganisationID }, organisation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrganisation(int id, Organisation organisation)
        {
            var outcome = await _organisationService.UpdateOrganisationAsync(id, organisation);

            return outcome switch
            {
                OrganisationOutcome.IdMismatch => this.ApiBadRequest("The ID in the URL does not match the organisation."),
                OrganisationOutcome.NotFound => NotFound(),
                OrganisationOutcome.DuplicateName => this.ApiConflict("An organisation with this name already exists."),
                _ => NoContent()
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganisation(int id)
        {
            return await _organisationService.DeleteOrganisationAsync(id) ? NoContent() : NotFound();
        }
    }
}