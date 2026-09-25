using System.Net.Http.Json;
using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    public class OrganisationService
    {
        private readonly HttpClient _httpClient;

        public OrganisationService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }
        //------------------------------------------------------------------------------//
        public async Task<List<OrganisationModel>> GetOrganisationsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<OrganisationModel>>("Organisations") ?? new List<OrganisationModel>();
        }
        //------------------------------------------------------------------------------//

        public async Task<OrganisationModel?> GetOrganisationAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<OrganisationModel>($"Organisations/{id}");
        }
        //------------------------------------------------------------------------------//

        public async Task<bool> CreateOrganisationAsync(OrganisationModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Organisations", model);

            return response.IsSuccessStatusCode;
        }
        //------------------------------------------------------------------------------//

        public async Task<bool> UpdateOrganisationAsync(OrganisationModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"Organisations/{model.OrganisationID}", model);

            return response.IsSuccessStatusCode;
        }
        //------------------------------------------------------------------------------//

        public async Task<bool> DeleteOrganisationAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Organisations/{id}");

            return response.IsSuccessStatusCode;
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//