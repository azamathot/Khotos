using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Headers;

namespace KhotosUI.Services
{
    public class JwtAuthorizationMessageHandler : DelegatingHandler
    {
        public static string JwtToken { get; private set; }
        private readonly IAccessTokenProvider _accessTokenProvider;
        public JwtAuthorizationMessageHandler(IAccessTokenProvider accessTokenProvider)
        {
            _accessTokenProvider = accessTokenProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Получение JWT-токена
            var accessTokenResult = await _accessTokenProvider.RequestAccessToken();

            // Проверка, получен ли токен 
            if (accessTokenResult.TryGetToken(out var accessToken))
            {
                JwtToken = accessToken.Value;
                // Добавление токена в заголовок Authorization (исправление)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);
            }

            // Отправка запроса к API
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
