using System.Net;
using System.Net.Http.Json;
using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    //This class sends user requests to the API.
    public class UserService
    {
        //The below line stores the HttpClient used to call the API.
        private readonly HttpClient _httpClient;

        //The constructor creates the HttpClient using the "BMSApi" settings.
        public UserService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }
        //------------------------------------------------------------------------------------//
        //This gets all the users from the API.
        public async Task<List<ViewUserModel>> GetUsersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ViewUserModel>>("Users") ?? new List<ViewUserModel>();
        }
        //------------------------------------------------------------------------------------//
        //This gets one user from the API using their ID.
        public async Task<ViewUserModel?> GetUserAsync(int id)
        {
            var response = await _httpClient.GetAsync($"Users/{id}");

            //This returns null if the user doesn't exist.
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ViewUserModel>();
        }
        //------------------------------------------------------------------------------------//
        //This sends a new user to the API to be created.
        public async Task<(bool Success, string Message)> CreateUserAsync(ViewUserModel model)
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

            //This returns if it worked and the message from the API.
            return (response.IsSuccessStatusCode,
                await ApiHelper.ReadMessageAsync(response, response.IsSuccessStatusCode ? "User created." : "Unable to create the user."));
        }
        //------------------------------------------------------------------------------------//
        //This sends the updated user details to the API.
        public async Task<(bool Success, string Message)> UpdateUserAsync(EditUserModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"Users/{model.UserID}", model);

            return (response.IsSuccessStatusCode,
                await ApiHelper.ReadMessageAsync(response, response.IsSuccessStatusCode ? "User updated." : "Unable to update the user."));
        }
        //------------------------------------------------------------------------------------//
        //This asks the API to delete a user.
        public async Task<(bool Success, string Message)> DeleteUserAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Users/{id}");

            return (response.IsSuccessStatusCode,
                await ApiHelper.ReadMessageAsync(response, response.IsSuccessStatusCode ? "User deleted." : "Unable to delete the user."));
        }
        //------------------------------------------------------------------------------------//
        //This asks the API to send a new verification email to a user.
        public async Task<(bool Success, string Message)> ResendVerificationAsync(int id)
        {
            var response = await _httpClient.PostAsync($"Users/{id}/resend-verification", null);

            return (response.IsSuccessStatusCode,
                await ApiHelper.ReadMessageAsync(response, response.IsSuccessStatusCode ? "Verification email sent." : "Unable to send the verification email."));
        }
    }
}
//-------------------------------------------<>\\<>END OF PROGRAM<>//<>-----------------------------------------//