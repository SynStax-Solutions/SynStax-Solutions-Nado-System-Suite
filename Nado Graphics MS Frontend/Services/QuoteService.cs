using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    // Thin HTTP wrapper around the backend Quotes API. Every method here maps
    // to one API endpoint.
    public class QuoteService
    {

        private readonly HttpClient _httpClient;

        //----------------------------------------------------------------------------------
        // The "BMSApi" named HttpClient is registered in Program.cs with its
        // BaseAddress pointing at the backend API (e.g. https://localhost:7030/api/).
        public QuoteService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }
        //----------------------------------------------------------------------------------
        // Fetches every quote for the Index/list page.
        public async Task<List<QuoteModel>> GetQuotesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<QuoteModel>>("Quotes") ?? new List<QuoteModel>();
        }
        //----------------------------------------------------------------------------------
        // Fetches a single quote by ID for the Details/Edit pages. Returns
        // null if not found (e.g. A 404).
        public async Task<QuoteModel?> GetQuoteAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<QuoteModel>($"Quotes/{id}");
        }
        //----------------------------------------------------------------------------------
        // Sends a new quote to the API.
        public async Task<(bool Success, string? Error)> CreateQuoteAsync(QuoteModel model)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(model);
            Console.WriteLine($"Sending to API: {json}");

            var response = await _httpClient.PostAsJsonAsync("Quotes", model);

            if (!response.IsSuccessStatusCode)
            {
                var rawBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API returned {(int)response.StatusCode}: {rawBody}");
                return (false, rawBody);
            }

            return (true, null);
        }
        //----------------------------------------------------------------------------------
        // Updates an existing quote.
        public async Task<bool> UpdateQuoteAsync(QuoteModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"Quotes/{model.QuoteID}", model);

            return response.IsSuccessStatusCode;
        }
        //----------------------------------------------------------------------------------//
        // Deletes a quote by ID.
        public async Task<bool> DeleteQuoteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Quotes/{id}");

            return response.IsSuccessStatusCode;
        }
        //----------------------------------------------------------------------------------//
        // Transitions a quote to Accepted via the backend's state-pattern
        // logic (see QuoteStateFactory on the backend). No request body is
        // needed - just the ID in the URL.
        public async Task<(bool Success, string? Error)> AcceptQuoteAsync(int id)
        {
            var response = await _httpClient.PostAsync($"Quotes/{id}/accept", null);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, error?.Message ?? "Unable to accept the quote.");
        }
        //----------------------------------------------------------------------------------//
        // Transitions a quote to Rejected, with a required reason string sent
        // as the raw JSON body (matching [FromBody] string reason on the API side).
        public async Task<(bool Success, string? Error)> RejectQuoteAsync(int id, string reason)
        {
            var response = await _httpClient.PostAsJsonAsync($"Quotes/{id}/reject", reason);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, error?.Message ?? "Unable to reject the quote.");
        }
        //----------------------------------------------------------------------------------//
        // Transitions a quote to Expired, same shape as RejectQuoteAsync.
        public async Task<(bool Success, string? Error)> ExpireQuoteAsync(int id, string reason)
        {
            var response = await _httpClient.PostAsJsonAsync($"Quotes/{id}/expire", reason);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, error?.Message ?? "Unable to expire the quote.");
        }
        //----------------------------------------------------------------------------------//
        //Matches the message shape returned by the API on a failed state transition.
        private class ErrorResponse
        {
            public string? Message { get; set; }
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------