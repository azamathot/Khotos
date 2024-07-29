using Keycloak.Net;
using Microsoft.Extensions.Configuration;

namespace KeycloakConfig
{
    public class KeycloakService : IKeycloakService
    {
        private readonly KeycloakSettings _settings;

        public KeycloakService(IConfiguration configuration)
        {
            KeycloakClient keycloakClient = new KeycloakClient("", "");
            _settings = configuration.GetSection("Keycloak").Get<KeycloakSettings>();
        }

        // Методы для получения ключей подписи, валидации токена и т.д.
        // ...
    }

    // KeycloakConfig/IKeycloakService.cs
    public interface IKeycloakService
    {
        // Методы для получения ключей подписи, валидации токена и т.д.
        // ...
    }
}
