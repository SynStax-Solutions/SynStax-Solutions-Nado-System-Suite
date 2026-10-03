using Nado_Graphics_MS_Frontend.Models;
using System.Net.Http.Json;
using System.Net.NetworkInformation;

namespace Nado_Graphics_MS_Frontend.Services
{
    //This class sends Job Card requests to the API. 
    public class JobCardService
    {
        //The below line stores the HttpClient used to call the API. 
        private readonly HttpClient _httpClient;

        //The constructor creates the HttpClient using the "BMSApi" settings. 
        public JobCardService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BMSApi");
        }

        //----------------------------------------------------------------------------------// 

        //This gets all Job Cards from the API. 
        public async Task<List<JobCardModel>> GetJobCardsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<JobCardModel>>("JobCards")
                ?? new List<JobCardModel>();
        }
        //----------------------------------------------------------------------------------// 

        public async Task<List<JobCardModel>> GetArchivedJobCardsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<JobCardModel>>("JobCards/archived")
                ?? new List<JobCardModel>();
        }
        //----------------------------------------------------------------------------------// 

        //This gets one Job Card from the API using its ID. 
        public async Task<JobCardModel?> GetJobCardAsync(int id)
        {
            var response = await _httpClient.GetAsync($"JobCards/{id}");

            //This returns null if the Job Card does not exist. 
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<JobCardModel>();
        }
        //----------------------------------------------------------------------------------// 

        //This gets all suppliers from the API. 
        public async Task<List<SupplierModel>> GetSuppliersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<SupplierModel>>("JobCards/suppliers")
                ?? new List<SupplierModel>();
        }
        //----------------------------------------------------------------------------------// 

        //This assigns a supplier to a Job Card through the API. 
        public async Task<bool> AssignSupplierAsync(int id, int supplierId)
        {
            var response = await _httpClient.PutAsJsonAsync($"JobCards/{id}/supplier", supplierId);

            return response.IsSuccessStatusCode;
        }
        //----------------------------------------------------------------------------------// 

        //This updates an existing Job Card through the API. 
        public async Task<bool> UpdateJobCardAsync(JobCardModel model)
        {
            //This sends the updated Job Card to the backend. 
            var response = await _httpClient.PutAsJsonAsync($"JobCards/{model.JobCardID}",
model);

            //This returns whether the update worked. 
            return response.IsSuccessStatusCode;
        }
        //------------------------------------------------------------------------------------------------------------------// 

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var response = await _httpClient.PutAsJsonAsync($"JobCards/{id}/status", status);

            return response.IsSuccessStatusCode;
        }
        //------------------------------------------------------------------------------------------------------------------// 

        public async Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus)
        {
            var response = await _httpClient.PutAsJsonAsync($"JobCards/{id}/paymentstatus",paymentStatus); 


            return response.IsSuccessStatusCode;
        }
    }
}
