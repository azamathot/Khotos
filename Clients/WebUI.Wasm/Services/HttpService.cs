using Microsoft.AspNetCore.Components;

namespace WebUI_Wasm.Services
{
    public class HttpService
    {
        public enum ServicesUrl { localhost, ProductsAPI, PortfolioAPI, PaynmentAPI, OrderingAPI, Auth2API, webui_wasm };
        Uri _uri;
        int _port;
        bool _isRunInContainer;
        IConfiguration _configuration;
        public HttpService(NavigationManager navigationManager, IConfiguration configuration)
        {
            _uri = new Uri(navigationManager.BaseUri);
            _isRunInContainer = Environment.GetEnvironmentVariable("HOSTNAME") != null;
            _port = _isRunInContainer && _uri.Scheme.Equals("http") ? 8080 : _isRunInContainer && _uri.Scheme.Equals("https") ? 8081 : _uri.Port;
            _configuration = configuration;
        }
        public HttpClient HttpClient(ServicesUrl baseUri = ServicesUrl.ProductsAPI)
        {
            string serviceNmae = Enum.GetName<ServicesUrl>(baseUri) ?? "localhost";
            var _baseuri = _configuration.GetSection("ServiceUrls")[serviceNmae];
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            // Pass the handler to httpclient(from you are calling api)
            var client = new HttpClient(clientHandler);
            client.BaseAddress = _isRunInContainer ? new Uri($"{_uri.Scheme}://{serviceNmae}:{_port}") : new Uri(_baseuri);
            return client;
        }
    }
}
