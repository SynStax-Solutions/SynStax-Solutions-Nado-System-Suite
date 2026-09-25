using System.Net.Http.Json;
using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }
        //------------------------------------------------------------------------------------//
        public async Task<List<ViewUserModel>> GetUsersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ViewUserModel>>("Users") ?? new List<ViewUserModel>();
        }
        //------------------------------------------------------------------------------------//
        public async Task<ViewUserModel?> GetUserAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ViewUserModel>($"Users/{id}");
        }
        //------------------------------------------------------------------------------------//
        public async Task<bool> CreateUserAsync(ViewUserModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Users",
                new
                {
                    model.FirstName,
                    model.LastName,
                    model.Email,
                    Password = model.Password,
                    model.UserRole
                });

            return response.IsSuccessStatusCode;
        }
        //------------------------------------------------------------------------------------//
        public async Task<bool> UpdateUserAsync(EditUserModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"Users/{model.UserID}", model);

            return response.IsSuccessStatusCode;
        }
        //------------------------------------------------------------------------------------//
        public async Task<bool> DeleteUserAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Users/{id}");

            return response.IsSuccessStatusCode;
        }
    }
}
//-------------------------------------------<>\\<>END OF PROGRAM<>//<>-----------------------------------------//