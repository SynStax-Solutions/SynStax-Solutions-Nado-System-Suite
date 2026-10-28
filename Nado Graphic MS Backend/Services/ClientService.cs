using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;

namespace Nado_Graphics_MS_Backend.Services
{
    //This class is the service that handles client business logic.
    public class ClientService
    {
        private readonly IRepository<Client> _clients;
        private readonly IOrganisationRepository _organisations;

        public ClientService(IRepository<Client> clients, IOrganisationRepository organisations)
        {
            _clients = clients;
            _organisations = organisations;
        }
        //----------------------------------------------------------------------------------//
        // This gets all the clients from the database.
        public async Task<List<Client>> GetClientsAsync()
        {
            //getting all client records and returning them as a list.
            return await _clients.GetAllAsync();
        }
        //----------------------------------------------------------------------------------//

        //Is used to get one client using the client's ID.
        public async Task<Client?> GetClientAsync(int id)
        {
            // Searches the database for the client with the specified ID.
            return await _clients.GetByIdAsync(id);
        }
        //----------------------------------------------------------------------------------//

        //The below code makes a new client and returns the result and an optional error.
        public async Task<bool> CreateClientAsync(Client client)
        {
            //This resets the client ID so the database generates a new ID.
            client.ClientID = 0;

            //This sets the creation date and time to the current date and time.
            client.CreatedAt = DateTime.Now;

            //This sets the current placeholder value for now
            client.PasswordHash = "N/A";

            //This checks whether the client is associated with an organisation.
            if (client.OrganisationID.HasValue)
            {
                //This searches for the organisation selected by the client.
                var organisation = await _organisations.GetByIdAsync(client.OrganisationID.Value);

                //This checks if the selected organisation does not exist.
                if (organisation == null)
                {
                    //This stops the process and returns an error message.
                    return false;
                }

                //This is used to store the organisation name on the client record.
                client.OrganisationName = organisation.OrganisationName;
            }
            else
            {
                //This is used to remove the organisation name when no organisation is selected.
                client.OrganisationName = null;
            }

            //Adds the new client to the db context.
            await _clients.AddAsync(client);
            await _clients.SaveChangesAsync();

            //Returns a successful result without an error message.
            return true;
        }
        //----------------------------------------------------------------------------------//

        //Updates an existing client and returns the operation result and an optional error.
        public async Task<bool> UpdateClientAsync(int id, Client client)
        {
            //The below if checks if the URL ID matches the client's ID.
            if (id != client.ClientID)
            {
                //This returns an unsuccessful result when the IDs do not match.
                return false;
            }

            //This searches the database for the existing client.
            var existingClient =
                await _clients.GetByIdAsync(id);

            //This checks whether the client does not exist.
            if (existingClient == null)
            {
                //Returns an unsuccessful result when the client is not found.
                return false;
            }

            //Checks if the client has an organisation selected.
            if (client.OrganisationID.HasValue)
            {
                //Searches for the selected organisation.
                var organisation = await _organisations.GetByIdAsync(client.OrganisationID.Value);

                //Checks if the selected organisation does not exist.
                if (organisation == null)
                {
                    //Returns an unsuccessful result with an error message.
                    return false;
                }

                //This updates the organisation ID on the existing client.
                existingClient.OrganisationID = client.OrganisationID;

                //This updates the organisation name on the existing client.
                existingClient.OrganisationName = organisation.OrganisationName;
            }
            else
            {
                //This removes the organisation ID when no organisation is selected.
                existingClient.OrganisationID = null;

                //This removes the organisation name when no organisation is selected.
                existingClient.OrganisationName = null;
            }

            //This updates the client's first name.
            existingClient.FirstName = client.FirstName;

            //This updates the client's last name.
            existingClient.LastName = client.LastName;

            //This updates the client's email address.
            existingClient.Email = client.Email;

            //This updates the client's main phone number.
            existingClient.PhoneNumber = client.PhoneNumber;

            //This updates the client's alternative phone number.
            existingClient.AlternativeNumber = client.AlternativeNumber;

            //This updates the client's type.
            existingClient.ClientType = client.ClientType;

            //This updates the client's street address.
            existingClient.StreetAddress = client.StreetAddress;

            //This updates the client's city.
            existingClient.City = client.City;

            //This updates the client's postal code.
            existingClient.PostalCode = client.PostalCode;

            //This checks whether a new password value is provided.
            if (!string.IsNullOrWhiteSpace(client.PasswordHash))
            {
                //This updates the password field when a value is provided.
                existingClient.PasswordHash = client.PasswordHash;
            }

            //This saves the updated client to the database.
            await _clients.SaveChangesAsync();

            //This returns a successful result without an error message.
            return true;
        }
        //----------------------------------------------------------------------------------//

        //This deletes an existing client and returns the operation result and an optional error.
        public async Task<bool> DeleteClientAsync(int id)
        {
            //Searching the database for the client using the specified ID.
            var client = await _clients.GetByIdAsync(id);

            //Checking if the client does not exist.
            if (client == null)
            {
                //Returns an unsuccessful result when the client is not found.
                return false;
            }

            //Marks the client for deletion in the db context.
            _clients.Remove(client);

            //Saving Changes
            await _clients.SaveChangesAsync();

            //Returns a successful result without an error message.
            return true;
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//