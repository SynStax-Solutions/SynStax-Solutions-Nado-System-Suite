using System.Net.Http.Headers;
using System.Net.Http.Json;
using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    public class PortfolioService
    {
        private readonly HttpClient _httpClient;

        public PortfolioService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }
        //----------------------------------------------------------------------------------
        public async Task<List<PortfolioSubmissionModel>> GetSubmissionsAsync(int? jobCardId = null)
        {
            var url = jobCardId != null ? $"PortfolioSubmissions?jobCardId={jobCardId}" : "PortfolioSubmissions";

            return await _httpClient.GetFromJsonAsync<List<PortfolioSubmissionModel>>(url)
                ?? new List<PortfolioSubmissionModel>();
        }
        //----------------------------------------------------------------------------------
        public async Task<(bool Success, string? Error)> CreateSubmissionAsync(CreatePortfolioSubmissionViewModel model)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(model.Title), "Title");

            if (!string.IsNullOrWhiteSpace(model.Description))
            {
                content.Add(new StringContent(model.Description), "Description");
            }

            if (model.JobCardID != null)
            {
                content.Add(new StringContent(model.JobCardID.Value.ToString()), "JobCardID");
            }

            await using var stream = model.MediaFile!.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.MediaFile.ContentType);
            content.Add(fileContent, "MediaFile", model.MediaFile.FileName);

            var response = await _httpClient.PostAsync("PortfolioSubmissions", content);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var body = await response.Content.ReadAsStringAsync();
            return (false, $"Upload failed ({(int)response.StatusCode}): {body}");
        }
        //----------------------------------------------------------------------------------
        public async Task<bool> DeleteSubmissionAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"PortfolioSubmissions/{id}");
            return response.IsSuccessStatusCode;
        }
        //----------------------------------------------------------------------------------
        public async Task<(byte[] Bytes, string ContentType)?> GetFileAsync(int id)
        {
            var response = await _httpClient.GetAsync($"PortfolioSubmissions/{id}/file");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            return (bytes, contentType);
        }
        //----------------------------------------------------------------------------------
        private class ErrorResponse
        {
            public string? Message { get; set; }
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------//