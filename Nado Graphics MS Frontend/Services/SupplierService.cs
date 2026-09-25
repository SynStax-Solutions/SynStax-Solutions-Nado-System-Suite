using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    public class SupplierService
    {
        private readonly HttpClient _httpClient;

        public SupplierService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }
        //------------------------------------------------------------------------------//
        public async Task<List<SupplierModel>> GetSuppliersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<SupplierModel>>("Suppliers") ?? new List<SupplierModel>();
        }
        //------------------------------------------------------------------------------//
        public async Task<SupplierModel?> GetSupplierAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<SupplierModel>($"Suppliers/{id}");
        }
        //------------------------------------------------------------------------------//
        public async Task<bool> CreateSupplierAsync(SupplierModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Suppliers", model);

            return response.IsSuccessStatusCode;
        }
        //------------------------------------------------------------------------------//
        public async Task<bool> UpdateSupplierAsync(SupplierModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"Suppliers/{model.SupplierID}", model);

            return response.IsSuccessStatusCode;
        }
        //------------------------------------------------------------------------------//
        public async Task<bool> DeleteSupplierAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Suppliers/{id}");

            return response.IsSuccessStatusCode;
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//