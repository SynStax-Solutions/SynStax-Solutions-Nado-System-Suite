using System.Net.Http.Json;
using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    //This class sends login and email verification requests to the API.
    public class AuthService
    {
        //The below line stores the HttpClient used to call the API.
        private readonly HttpClient _httpClient;

        //The constructor creates the HttpClient using the "BMSApi" settings.
        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }

        //------------------------------------------------------------------------------------//
        //This sends the email and password to the API to log in.
        public async Task<(LoginResponseModel? User, string? Error)> LoginAsync(string email, string password)
        {
            try
            {
                //This sends the login details to /api/Auth/login.
                var response = await _httpClient.PostAsJsonAsync("Auth/login", new { email, password });

                //If the login worked, the user's details are returned.
                if (response.IsSuccessStatusCode)
                {
                    return (await response.Content.ReadFromJsonAsync<LoginResponseModel>(), null);
                }

                //If it failed, the error message is returned.
                return (null, await ApiHelper.ReadMessageAsync(response, "Invalid email or password."));
            }
            catch (HttpRequestException)
            {
                //This happens if the backend is not running.
                return (null, "Cannot reach the server. Make sure the backend API is running.");
            }
        }

        //------------------------------------------------------------------------------------//
        //This sends the token from the email link to the API to verify the email.
        public async Task<(bool Success, string Message)> VerifyEmailAsync(string token)
        {
            try
            {
                //This sends the token to /api/Auth/verify-email.
                var response = await _httpClient.PostAsJsonAsync("Auth/verify-email", new { token });

                //This gets the message to show the user.
                var message = await ApiHelper.ReadMessageAsync(response,
                    response.IsSuccessStatusCode ? "Email verified." : "This verification link is invalid.");

                return (response.IsSuccessStatusCode, message);
            }
            catch (HttpRequestException)
            {
                //This happens if the backend is not running.
                return (false, "Cannot reach the server. Please try again later.");
            }
        }

        //------------------------------------------------------------------------------------//
        //This sends the current and new password to the API to change the user's password.
        public async Task<(bool Success, string Message)> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                //This sends the passwords to /api/Account/change-password.
                //The user's token is added automatically by the ApiTokenHandler.
                var response = await _httpClient.PostAsJsonAsync("Account/change-password", new { currentPassword, newPassword });

                //This gets the message to show the user.
                var message = await ApiHelper.ReadMessageAsync(response,
                    response.IsSuccessStatusCode ? "Your password has been changed." : "Unable to change your password.");

                return (response.IsSuccessStatusCode, message);
            }
            catch (HttpRequestException)
            {
                //This happens if the backend is not running.
                return (false, "Cannot reach the server. Please try again later.");
            }
        }

        //------------------------------------------------------------------------------------//
        //This asks the API to email a password reset link.
        public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email)
        {
            try
            {
                //This sends the email to /api/Auth/forgot-password.
                var response = await _httpClient.PostAsJsonAsync("Auth/forgot-password", new { email });

                //This gets the message to show the user.
                var message = await ApiHelper.ReadMessageAsync(response,
                    response.IsSuccessStatusCode
                        ? "If an account exists for that email address, a password reset link has been sent to it."
                        : "Unable to send a reset link. Please try again.");

                return (response.IsSuccessStatusCode, message);
            }
            catch (HttpRequestException)
            {
                //This happens if the backend is not running.
                return (false, "Cannot reach the server. Please try again later.");
            }
        }

        //------------------------------------------------------------------------------------//
        //This sends the token from the reset link and the new password to the API.
        public async Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                //This sends the details to /api/Auth/reset-password.
                var response = await _httpClient.PostAsJsonAsync("Auth/reset-password", new { token, newPassword });

                //This gets the message to show the user.
                var message = await ApiHelper.ReadMessageAsync(response,
                    response.IsSuccessStatusCode ? "Your password has been reset." : "This password reset link is invalid or has expired.");

                return (response.IsSuccessStatusCode, message);
            }
            catch (HttpRequestException)
            {
                //This happens if the backend is not running.
                return (false, "Cannot reach the server. Please try again later.");
            }
        }
    }
}

//-------------------------------------------<>\\<>END OF PROGRAM<>//<>-----------------------------------------//