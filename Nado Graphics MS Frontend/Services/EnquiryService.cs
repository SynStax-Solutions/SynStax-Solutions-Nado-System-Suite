using System.Net.Http.Json;
using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    public class EnquiryService
    {
        private readonly HttpClient _httpClient;

        public EnquiryService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }
        //----------------------------------------------------------------------------------//
        public async Task<List<EnquiryModel>> GetEnquiriesAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<EnquiryModel>>("Enquiries")
                       ?? new List<EnquiryModel>();
            }
            catch
            {
                return new List<EnquiryModel>();
            }
        }
        //----------------------------------------------------------------------------------//
        public async Task<EnquiryModel?> GetEnquiryAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EnquiryModel>($"Enquiries/{id}");
        }
        //----------------------------------------------------------------------------------//
        public async Task<bool> MarkAsReadAsync(int id)
        {
            var response = await _httpClient.PutAsync($"Enquiries/{id}/read", null);
            return response.IsSuccessStatusCode;
        }
    }
}