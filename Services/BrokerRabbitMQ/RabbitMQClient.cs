using RabbitMQ.Client;

namespace BrokerRabbitMQ
{
    public class RabbitMQClient : IRabbitMQClient
    {
        private readonly IConfiguration _configuration;

        public RabbitMQClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void PublishMessage(string queueName, string message)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration.GetValue<string>("RabbitMQ:HostName"),
                Port = _configuration.GetValue<int>("RabbitMQ:Port"),
                UserName = _configuration.GetValue<string>("RabbitMQ:UserName"),
                Password = _configuration.GetValue<string>("RabbitMQ:Password")
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queueName, true, false, false, null);

            var body = System.Text.Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        }
    }
}
