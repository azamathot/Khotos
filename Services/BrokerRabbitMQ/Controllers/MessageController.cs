using Microsoft.AspNetCore.Mvc;

namespace BrokerRabbitMQ.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IRabbitMQClient _rabbitMQClient;
        private readonly IConfiguration _configuration;

        public MessageController(IRabbitMQClient rabbitMQClient, IConfiguration configuration)
        {
            _rabbitMQClient = rabbitMQClient;
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult SendMessage(string message)
        {
            _rabbitMQClient.PublishMessage(_configuration["RabbitMq:QueueName"], message);

            return Ok("Сообщение отправлено");
        }
    }
}
