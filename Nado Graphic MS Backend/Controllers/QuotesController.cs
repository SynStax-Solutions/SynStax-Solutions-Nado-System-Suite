using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Services;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Security;

namespace Nado_Graphics_MS_Backend.Controllers
{
    // enables automatic model validation
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class QuotesController : Controller
    {
        private readonly QuoteService _quoteService;
        
        public QuotesController(QuoteService quoteService)
        {
            _quoteService = quoteService;
        }
        //--------------------------------------------------------------------------------------------------------------//
        // GET api/Quotes - returns every quote.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quote>>> GetQuotes()
        {
            var quotes = await _quoteService.GetQuotesAsync();
            return Ok(quotes);
        }
        //--------------------------------------------------------------------------------------------------------------//
        // GET api/Quotes/{id} - returns one quote or 404.
        [HttpGet("{id}")]
        public async Task<ActionResult<Quote>> GetQuote(int id)
        {
            var quote = await _quoteService.GetQuoteAsync(id);

            if (quote == null)
            {
                return NotFound();
            }

            return Ok(quote);
        }
        //--------------------------------------------------------------------------------------------------------------//
        // POST api/Quotes - creates a new quote.
        [HttpPost]
        public async Task<ActionResult<Quote>> CreateQuote(Quote quote)
        {
            var (success, error) = await _quoteService.CreateQuoteAsync(quote);

            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return CreatedAtAction(nameof(GetQuote), new { id = quote.QuoteID }, quote);
        }
        //--------------------------------------------------------------------------------------------------------------
        // PUT api/Quotes/{id} - updates an existing (Pending) quote.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuote(int id, Quote quote)
        {
            var success = await _quoteService.UpdateQuoteAsync(id, quote);

            if (!success)
            {
                return BadRequest();
            }

            return NoContent();
        }
        //--------------------------------------------------------------------------------------------------------------
        // DELETE api/Quotes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuote(int id)
        {
            var success = await _quoteService.DeleteQuoteAsync(id);

            if (!success)
            {
                return NotFound();
            }

            // 204 No Content
            // meaningful to return in the body.
            return NoContent();
        }

        //--------------------------------------------------------------------------------------------------------------
        //Handles POST requests sent to /api/Quotes/{id}/accept.
        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptQuote(int id)
        {
            var (success, error) = await _quoteService.AcceptQuoteAsync(id);

            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return NoContent();
        }
        //--------------------------------------------------------------------------------------------------------------
        //Handles POST requests sent to /api/Quotes/{id}/reject.
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectQuote(int id, [FromBody] string reason)
        {
            var (success, error) = await _quoteService.RejectQuoteAsync(id, reason);

            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return NoContent();
        }
        //--------------------------------------------------------------------------------------------------------------
        //Handles POST requests sent to /api/Quotes/{id}/expire.
        [HttpPost("{id}/expire")]
        public async Task<IActionResult> ExpireQuote(int id, [FromBody] string reason)
        {
            var (success, error) = await _quoteService.ExpireQuoteAsync(id, reason);

            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return NoContent();
        }


    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------