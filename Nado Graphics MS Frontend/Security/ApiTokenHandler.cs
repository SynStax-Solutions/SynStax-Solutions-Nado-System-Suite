using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Nado_Graphics_MS_Frontend.Security
{
    //This class adds the logged-in user's token to every request sent to the API.
    //Because of this, the other services don't need to be changed.
    public class ApiTokenHandler : DelegatingHandler
    {
        //This is the name used to store the token in the login cookie.
        public const string TokenClaim = "access_token";

        //The below line lets us get the logged-in user.
        private readonly IHttpContextAccessor _httpContextAccessor;

        //The constructor receives it using dependency injection.
        public ApiTokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        //This runs before every API request is sent.
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            //This gets the token from the logged-in user.
            var token = httpContext?.User.FindFirst(TokenClaim)?.Value;

            //If there is a token, it is added to the request header.
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            //This sends the request on to the API.
            var response = await base.SendAsync(request, cancellationToken);

            //If the API says the user is no longer allowed in (e.g. deactivated),
            //the user is logged out of the website as well.
            if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(token) && httpContext != null)
            {
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return response;
        }
    }
}
/* References
 Microsoft (n.d.) Make HTTP requests using IHttpClientFactory in ASP.NET Core: Outgoing request middleware. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#outgoing-request-middleware

Jones, M. and Hardt, D. (2012) RFC 6750: The OAuth 2.0 Authorization Framework: Bearer Token Usage. Internet Engineering Task Force. Available at: https://www.rfc-editor.org/rfc/rfc6750
 */
// ------------------------- END OF FILE ------------------------- //