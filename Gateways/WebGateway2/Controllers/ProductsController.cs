using Microsoft.AspNetCore.Mvc;

namespace WebGateway2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        IConfiguration _configuration;
        public ProductsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("gethello")]
        public string GetHello() => "hello world";

        [HttpGet("getcategorieslink")]
        public async Task<string> GetCategoriesLink()
        {
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            };

            try
            {
                HttpClient client = new HttpClient(httpHandler);
                var responce = await client.GetAsync("https://productsapi:8081/products/Categories/getcategorieslink");
                if (responce != null && responce.IsSuccessStatusCode)
                {
                    var t = await responce.Content.ReadAsStringAsync();
                    return t;
                }
            }
            catch (Exception ex) { }
            try
            {
                HttpClient client = new HttpClient(httpHandler);
                var responce = await client.GetAsync("https://localhost:7001/products/Categories/getcategorieslink");
                if (responce != null && responce.IsSuccessStatusCode)
                {
                    var t = await responce.Content.ReadAsStringAsync();
                    return t;
                }
            }
            catch (Exception ex) { }
            return "not found";
        }
    }
}
