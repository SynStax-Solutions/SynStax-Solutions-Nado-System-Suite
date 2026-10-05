using System.Net.Http.Json;
using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    public class ClientService
    {
        private readonly HttpClient _httpClient;

        public ClientService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }
        //----------------------------------------------------------------------------------//
        public async Task<List<ClientModel>> GetClientsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ClientModel>>("Clients") ?? new List<ClientModel>();
        }
        //----------------------------------------------------------------------------------//
        public async Task<ClientModel?> GetClientAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ClientModel>($"Clients/{id}");
        }
        //----------------------------------------------------------------------------------//
        public async Task<bool> CreateClientAsync(ClientModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Clients", model);

            return response.IsSuccessStatusCode;
        }
        //----------------------------------------------------------------------------------//
        public async Task<bool> UpdateClientAsync(ClientModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"Clients/{model.ClientID}", model);

            return response.IsSuccessStatusCode;
        }
        //----------------------------------------------------------------------------------//
        public async Task<bool> DeleteClientAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Clients/{id}");

            return response.IsSuccessStatusCode;
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//