using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace WebGateway.Configuration
{
    public class OcelotConfig
    {
        public static void LoadConfig(ConfigurationManager config)
        {
            config.AddJsonFile($"ocelot.{config["ENVIRONMENT"]}.json", optional: false, reloadOnChange: true);
            if (config["ENVIRONMENT"] == "Development"
                && config["DOTNET_RUNNING_IN_CONTAINER"] == "true")
            {
                config.AddJsonFile("ocelot.DockerDev.json", optional: true, reloadOnChange: true);
            }
        }

        public static bool MyCustomValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Проверяем, что сертификат соответствует имени хоста бэкенд-сервиса
            //if (certificate.Subject.Contains("CN=your-backend-hostname"))
            if (certificate.Subject.Contains("CN=khotos.ru"))
            {
                return true;
            }

            return false;
        }
    }
}
