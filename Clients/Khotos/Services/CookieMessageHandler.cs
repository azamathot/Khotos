using Microsoft.AspNetCore.Authentication;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Net.Http.Headers;

namespace Khotos.Services
{
    public class CookieMessageHandler : DelegatingHandler
    {
        private readonly CookieContainer _cookies = new CookieContainer();
        private readonly IHttpContextAccessor _httpContextAccessor;
        public static string JwtToken { get; private set; }

        public CookieMessageHandler(IHttpContextAccessor httpContextAccessor)
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

            request.Headers.Add("Cookie", _cookies.GetCookieHeader(request.RequestUri));
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.Headers.TryGetValues("Set-Cookie", out var newCookies))
            {
                foreach (var item in SetCookieHeaderValue.ParseList(newCookies.ToList()))
                {
                    var uri = new Uri(request.RequestUri, item.Path.Value);
                    _cookies.Add(uri, new Cookie(item.Name.Value, item.Value.Value, item.Path.Value));
                }
            }
            return response;
        }
    }
}
