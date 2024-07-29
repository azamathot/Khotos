namespace WebGateway.Configuration
{
    public class KeycloakOptions
    {
        public string Authority { get; set; }
        public string Audience { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public List<string> Scopes { get; set; } = new();
    }
}
