using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ProductsAPI.MQ
{
    public class RabbitMqConsumerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMqConsumerService> _logger;

        public RabbitMqConsumerService(IConfiguration configuration, ILogger<RabbitMqConsumerService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool _isRunInContainer = Environment.GetEnvironmentVariable("HOSTNAME") != null;
            // Настройка соединения с RabbitMQ
            var factory = new ConnectionFactory()
            {
                HostName = _isRunInContainer ? _configuration["RabbitMq:HostNameDocker"] : _configuration["RabbitMq:HostName"],
                UserName = _configuration["RabbitMq:UserName"],
                Password = _configuration["RabbitMq:Password"]
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Объявление очереди (если она не существует)
            channel.QueueDeclare(
                queue: _configuration["RabbitMq:QueueName"],
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Подписка на очередь
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation($"Получено сообщение: {message}");

                // Обработка сообщения (запись в базу данных, отправка уведомления и т.д.)

                // Подтверждение получения сообщения
                channel.BasicAck(ea.DeliveryTag, false);
            };
            channel.BasicConsume(_configuration["RabbitMq:QueueName"], false, consumer);

            // Ожидание завершения работы сервиса
            await Task.Delay(-1, stoppingToken);
        }
    }
}
