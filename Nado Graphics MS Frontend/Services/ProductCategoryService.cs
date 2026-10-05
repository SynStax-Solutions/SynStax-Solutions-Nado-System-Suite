using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    public class ProductCategoryService
    {
        private readonly HttpClient _httpClient;

        public ProductCategoryService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }
        //----------------------------------------------------------------------------------//
        public async Task<List<ProductCategoryModel>> GetCategoriesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ProductCategoryModel>>("ProductCategories") ?? new List<ProductCategoryModel>();
        }
        //----------------------------------------------------------------------------------//
        //Returns the created category (with its new ID) so the calling page can select it immediately.
        public async Task<ProductCategoryModel?> CreateCategoryAsync(ProductCategoryModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("ProductCategories", model);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ProductCategoryModel>();
        }
    }
}
