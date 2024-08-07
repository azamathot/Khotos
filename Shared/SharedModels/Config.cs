using Microsoft.Extensions.Configuration;

namespace SharedModels
{
    public class Config
    {
        public static string GetConnectionString(IConfiguration config)
        {
            //bool _isRunInContainer = Environment.GetEnvironmentVariable("HOSTNAME") != null;
            //string ConnectNameString = _isRunInContainer ? "DefaultConnectionContainerIn" : "DefaultConnectionLocalDb";
            //var isRunInDocker = builder.Configuration["DOTNET_RUNNING_IN_CONTAINER"];
            //var isRunInDocker1 = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
            string password = config["PasswordDb"];
            string connectString = config.GetConnectionString("DefaultConnection")?.Replace("[password]", password);
            return connectString ?? throw new InvalidOperationException($"Connection string 'DefaultConnection' not found.");
        }

        public static void ConfigAppConfiguration(ConfigurationManager config)
        {
            if (config["ENVIRONMENT"] == "Development"
                && config["DOTNET_RUNNING_IN_CONTAINER"] == "true")
            {
                config.AddJsonFile("appsettings.DockerDev.json", optional: true, reloadOnChange: true);
            }
        }

        public static void ConfigAppConfiguration<T>(T config) where T : IConfiguration
        {
            // Получаем IConfigurationBuilder
            IConfigurationBuilder builder = config as IConfigurationBuilder;

            if (builder != null && config["ENVIRONMENT"] == "Development" && config["DOTNET_RUNNING_IN_CONTAINER"] == "true")
            {
                //builder.AddJsonFile("appsettings.DockerDev.json", optional: true, reloadOnChange: true);
                builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            }
        }
    }
}
