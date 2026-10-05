using System.Net.Http.Json;
using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    //This service sends Organisation requests from the frontend to the backend API.
    public class OrganisationService
    {
        //This stores the HttpClient used to communicate with the backend API.
        private readonly HttpClient _httpClient;

        //This creates the HttpClient using the configured BMS API client.
        public OrganisationService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }
        //------------------------------------------------------------------------------//

        //This gets all Organisations from the backend API.
        public async Task<List<OrganisationModel>> GetOrganisationsAsync()
        {
            //This sends a GET request to the Organisations endpoint.
            return await _httpClient.GetFromJsonAsync<List<OrganisationModel>>("Organisations") ?? new List<OrganisationModel>();
        }
        //------------------------------------------------------------------------------//

        //This gets one Organisation using its ID.
        public async Task<OrganisationModel?> GetOrganisationAsync(int id)
        {
            //This sends a GET request for the selected Organisation.
            return await _httpClient.GetFromJsonAsync<OrganisationModel>($"Organisations/{id}");
        }
        //------------------------------------------------------------------------------//

        //This creates a new Organisation through the backend API.
        public async Task<bool> CreateOrganisationAsync(OrganisationModel model)
        {
            //This sends the new Organisation to the backend.
            var response = await _httpClient.PostAsJsonAsync("Organisations", model);

            //This returns whether the API reports the request as successful.
            return response.IsSuccessStatusCode;
        }
        //------------------------------------------------------------------------------//

        //This updates an existing Organisation through the backend API.
        public async Task<bool> UpdateOrganisationAsync(OrganisationModel model)
        {
            //This sends the updated Organisation to the backend using its ID.
            var response = await _httpClient.PutAsJsonAsync($"Organisations/{model.OrganisationID}", model);

            //This returns whether the API reports the update as successful.
            return response.IsSuccessStatusCode;
        }
        //------------------------------------------------------------------------------//

        //This deletes an Organisation through the backend API.
        public async Task<bool> DeleteOrganisationAsync(int id)
        {
            //This sends a DELETE request for the selected Organisation.
            var response = await _httpClient.DeleteAsync($"Organisations/{id}");

            //This returns whether the API reports the deletion as successful.
            return response.IsSuccessStatusCode;
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//