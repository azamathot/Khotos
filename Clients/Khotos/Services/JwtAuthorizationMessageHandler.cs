using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace Khotos.Services
{
    public class JwtAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public static string JwtToken { get; private set; }
        public JwtAuthorizationMessageHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

            if (!string.IsNullOrEmpty(accessToken))
            {
                JwtToken = accessToken;
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            //request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            //request.Headers.Add("X-Requested-With", ["XMLHttpRequest"]);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
