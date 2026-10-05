using System.Net.Http.Json;
using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    public class StatementService
    {
        private readonly HttpClient _httpClient;

        public StatementService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }
        //----------------------------------------------------------------------------------
        public async Task<List<StatementModel>> GetStatementsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<StatementModel>>("Statements") ?? new List<StatementModel>();
        }
        //----------------------------------------------------------------------------------
        public async Task<StatementDetailsModel?> GetStatementAsync(int id)
        {
            var response = await _httpClient.GetAsync($"Statements/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<StatementDetailsModel>();
        }
        //----------------------------------------------------------------------------------
        public async Task<(bool Success, string? Error, StatementModel? Statement)> CreateStatementAsync(CreateStatementRequestModel request)
        {
            var response = await _httpClient.PostAsJsonAsync("Statements", request);

            if (response.IsSuccessStatusCode)
            {
                var statement = await response.Content.ReadFromJsonAsync<StatementModel>();
                return (true, null, statement);
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, error?.Message ?? "Unable to create the statement.", null);
        }
        //----------------------------------------------------------------------------------
        public async Task<bool> DeleteStatementAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Statements/{id}");
            return response.IsSuccessStatusCode;
        }
        //----------------------------------------------------------------------------------
        public async Task<byte[]?> DownloadStatementPdfAsync(int id)
        {
            var response = await _httpClient.GetAsync($"Statements/{id}/pdf");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        //----------------------------------------------------------------------------------
        private class ErrorResponse
        {
            public string? Message { get; set; }
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------//