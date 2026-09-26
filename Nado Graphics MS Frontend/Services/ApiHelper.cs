using System.Net.Http.Json;
using Nado_Graphics_MS_Frontend.Models;

namespace Nado_Graphics_MS_Frontend.Services
{
    //This class has helper methods used by the services.
    public static class ApiHelper
    {
        //This reads the message from the API response.
        //If there is no message, the fallback text is used instead.
        public static async Task<string> ReadMessageAsync(HttpResponseMessage response, string fallback)
        {
            try
            {
                var body = await response.Content.ReadFromJsonAsync<ApiMessage>();
                return string.IsNullOrWhiteSpace(body?.Message) ? fallback : body.Message;
            }
            catch
            {
                return fallback;
            }
        }
    }
}
// ------------------------- END OF FILE ------------------------- //