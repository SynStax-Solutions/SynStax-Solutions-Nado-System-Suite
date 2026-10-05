using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    //This service sends Supplier requests from the frontend to the backend API.
    public class SupplierService
    {
        //This stores the HttpClient used to communicate with the backend API.
        private readonly HttpClient _httpClient;

        //This creates the HttpClient using the configured BMS API client.
        public SupplierService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }
        //------------------------------------------------------------------------------//

        //This gets all Suppliers from the backend API.
        public async Task<List<SupplierModel>> GetSuppliersAsync()
        {
            //This sends a GET request to the Suppliers endpoint.
            return await _httpClient.GetFromJsonAsync<List<SupplierModel>>("Suppliers") ?? new List<SupplierModel>();
        }
        //------------------------------------------------------------------------------//

        //This gets one Supplier using its ID.
        public async Task<SupplierModel?> GetSupplierAsync(int id)
        {
            //This sends a GET request for the selected Supplier.
            return await _httpClient.GetFromJsonAsync<SupplierModel>($"Suppliers/{id}");
        }
        //------------------------------------------------------------------------------//

        //This creates a new Supplier through the backend API.
        public async Task<bool> CreateSupplierAsync(SupplierModel model)
        {
            //This sends the new Supplier to the backend.
            var response = await _httpClient.PostAsJsonAsync("Suppliers", model);

            //This returns whether the API reports the request as successful.
            return response.IsSuccessStatusCode;
        }
        //------------------------------------------------------------------------------//

        //This updates an existing Supplier through the backend API.
        public async Task<bool> UpdateSupplierAsync(SupplierModel model)
        {
            //This sends the updated Supplier to the backend using its ID.
            var response = await _httpClient.PutAsJsonAsync($"Suppliers/{model.SupplierID}", model);

            //This returns whether the API reports the update as successful.
            return response.IsSuccessStatusCode;
        }
        //------------------------------------------------------------------------------//

        //This deletes a Supplier through the backend API.
        public async Task<bool> DeleteSupplierAsync(int id)
        {
            //This sends a DELETE request for the selected Supplier.
            var response = await _httpClient.DeleteAsync($"Suppliers/{id}");

            //This returns whether the API reports the deletion as successful.
            return response.IsSuccessStatusCode;
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//