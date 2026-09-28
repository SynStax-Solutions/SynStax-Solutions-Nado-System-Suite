using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;

        public ProductService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }
        //----------------------------------------------------------------------------------//
        public async Task<List<ProductModel>> GetProductsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ProductModel>>("Products") ?? new List<ProductModel>();
        }
        //----------------------------------------------------------------------------------//
        public async Task<ProductModel?> GetProductAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ProductModel>($"Products/{id}");
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------