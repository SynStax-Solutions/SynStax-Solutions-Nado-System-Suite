using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Security;
using Nado_Graphics_MS_Backend.Services;

namespace Nado_Graphics_MS_Backend.Controllers
{
    // Create, list, view, download (PDF) and delete statements. Secured like InvoicesController.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class StatementsController : ControllerBase
    {
        private readonly StatementService _statementService;
        private readonly StatementPDFService _statementPdfService;

        public StatementsController(StatementService statementService, StatementPDFService statementPdfService)
        {
            _statementService = statementService;
            _statementPdfService = statementPdfService;
        }
        //--------------------------------------------------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Statement>>> GetStatements()
        {
            var statements = await _statementService.GetStatementsAsync();
            return Ok(statements);
        }
        //--------------------------------------------------------------------------------------------------------------
        // Returns the statement with its calculated lines and totals.
        [HttpGet("{id}")]
        public async Task<ActionResult<StatementData>> GetStatement(int id)
        {
            var data = await _statementService.GetStatementDataAsync(id);

            if (data == null)
            {
                return NotFound();
            }

            return Ok(data);
        }
        //--------------------------------------------------------------------------------------------------------------
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var salesRepName = User.FindFirst(AppClaims.Name)?.Value ?? "Nado Graphics";
            var pdf = await _statementPdfService.GenerateStatementPdfAsync(id, salesRepName);

            if (pdf == null)
            {
                return NotFound();
            }

            return File(pdf, "application/pdf", $"Statement-{id}.pdf");
        }
        //--------------------------------------------------------------------------------------------------------------
        // Creates a statement for a client OR an organisation over the requested period.
        [HttpPost]
        public async Task<ActionResult<Statement>> CreateStatement(CreateStatementRequest request)
        {
            var (success, error, statement) = await _statementService.CreateStatementAsync(request);

            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return CreatedAtAction(nameof(GetStatement), new { id = statement!.StatementID }, statement);
        }
        //--------------------------------------------------------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStatement(int id)
        {
            var success = await _statementService.DeleteStatementAsync(id);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------