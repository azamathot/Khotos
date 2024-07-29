using Microsoft.Extensions.Configuration;

namespace SharedModels
{
    public class ConnectDb
    {
        public static string GetConnectionString(IConfiguration config)
        {
            bool _isRunInContainer = Environment.GetEnvironmentVariable("HOSTNAME") != null;
            string ConnectNameString = _isRunInContainer ? "DefaultConnectionContainerIn" : "DefaultConnectionLocalDb";
            return config.GetConnectionString(ConnectNameString) ?? throw new InvalidOperationException($"Connection string '{ConnectNameString}' not found.");
        }
    }
}
