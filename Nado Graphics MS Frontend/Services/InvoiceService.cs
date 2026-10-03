using System.Net.Http.Json;
using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    public class InvoiceService
    {
        private readonly HttpClient _httpClient;

        public InvoiceService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }
        //----------------------------------------------------------------------------------
        public async Task<List<InvoiceModel>> GetInvoicesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<InvoiceModel>>("Invoices") ?? new List<InvoiceModel>();
        }
        //----------------------------------------------------------------------------------
        public async Task<InvoiceModel?> GetInvoiceAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<InvoiceModel>($"Invoices/{id}");
        }
        //----------------------------------------------------------------------------------
        public async Task<bool> CreateInvoiceManuallyAsync(InvoiceModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Invoices/manually", model);
            return response.IsSuccessStatusCode;
        }
        //----------------------------------------------------------------------------------
        public async Task<(bool Success, string? Error)> CreateInvoiceFromQuoteAsync(int quoteId)
        {
            var response = await _httpClient.PostAsync($"Invoices/from-quote/{quoteId}", null);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, error?.Message ?? "Unable to create invoice from quote.");
        }
        //----------------------------------------------------------------------------------
        public async Task<bool> UpdateInvoiceAsync(InvoiceModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"Invoices/{model.InvoiceID}", model);
            return response.IsSuccessStatusCode;
        }
        //----------------------------------------------------------------------------------
        public async Task<bool> DeleteInvoiceAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Invoices/{id}");
            return response.IsSuccessStatusCode;
        }
        //----------------------------------------------------------------------------------
        public async Task<(bool Success, string? Error)> RecordPaymentAsync(int id, RecordPaymentRequestModel model)
        {
            var response = await _httpClient.PostAsJsonAsync($"Invoices/{id}/payments", model);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, error?.Message ?? "Unable to record payment.");
        }
        //----------------------------------------------------------------------------------
        public async Task<(bool Success, string? Error)> ExtendInvoiceAsync(int id, string reason)
        {
            var response = await _httpClient.PutAsJsonAsync($"Invoices/{id}/extension", new { reason });

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, error?.Message ?? "Unable to extend invoice.");
        }
        //----------------------------------------------------------------------------------
        public async Task<(bool Success, string? Error)> CancelInvoiceAsync(int id, string reason)
        {
            var response = await _httpClient.PutAsJsonAsync($"Invoices/{id}/cancel", new { reason });

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, error?.Message ?? "Unable to cancel invoice.");
        }
        //----------------------------------------------------------------------------------
        private class ErrorResponse
        {
            public string? Message { get; set; }
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------//