using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Nado_Graphics_MS_Backend.Security;

namespace Nado_Graphics_MS_Backend.Controllers
{
    //Marks the class as an API controller.
    [ApiController]

    //Defines the API route as /api/Clients.
    [Route("api/[controller]")]

    //Only a logged-in Manager (Owner) or PA/Admin can use these endpoints.
    [Authorize(Roles = AppRoles.ManagerOrAdmin)]
    public class ClientsController : ControllerBase
    {
        //The below line stores the ClientService used to handle client operations.
        private readonly ClientService _clientService;

        //The constructor receives the ClientService using dependency injection.
        public ClientsController(ClientService clientService)
        {
            //Storing the injected ClientService.
            _clientService = clientService;
        }
        //--------------------------------------------------------------------------------------------------------------//
        //Handles GET requests sent to /api/Clients.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            //Getting all clients from the ClientService.
            var clients = await _clientService.GetClientsAsync();

            //Returning the clients with an HTTP 200 OK response.
            return Ok(clients);
        }
        //--------------------------------------------------------------------------------------------------------------//
        //Handles GET requests sent to /api/Clients/{id}.
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            //Getting the selected client from the ClientService.
            var client = await _clientService.GetClientAsync(id);

            //Checking if the client does not exist.
            if (client == null)
            {
                //Returning an HTTP 404 Not Found response.
                return NotFound();
            }

            //Returning the client with an HTTP 200 OK response.
            return Ok(client);
        }
        //--------------------------------------------------------------------------------------------------------------//
        //Handles POST requests sent to /api/Clients.
        [HttpPost]
        public async Task<ActionResult<Client>> CreateClient(Client client)
        {
            //Sending the client to the ClientService for creation.
            var success = await _clientService.CreateClientAsync(client);

            //Checking if the client creation fails.
            if (!success)
            {
                //Returning an HTTP 400 Bad Request response.
                return BadRequest();
            }

            //Returning the newly created client with an HTTP 201 Created response.
            return CreatedAtAction(nameof(GetClient), new { id = client.ClientID }, client);
        }
        //--------------------------------------------------------------------------------------------------------------//
        //Handles PUT requests sent to /api/Clients/{id}.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id,Client client)
        {
            //Sending the client to the ClientService for updating.
            var success = await _clientService.UpdateClientAsync(
                id,
                client);

            //Checking if the client update fails.
            if (!success)
            {
                //Returning an HTTP 400 Bad Request response.
                return BadRequest();
            }

            //Returning an HTTP 204 No Content response.
            return NoContent();
        }
        //--------------------------------------------------------------------------------------------------------------//
        //Handles DELETE requests sent to /api/Clients/{id}.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            //Sending the client ID to the ClientService for deletion.
            var success = await _clientService.DeleteClientAsync(id);

            //Checking if the client deletion fails.
            if (!success)
            {
                //Returning an HTTP 404 Not Found response.
                return NotFound();
            }

            //Returning an HTTP 204 No Content response.
            return NoContent();
        }
    }
}
/* References
 Microsoft (n.d.) Role-based authorization in ASP.NET Core. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles
 */
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//