using Microsoft.AspNetCore.Mvc;

namespace BrokerRabbitMQ.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IRabbitMQClient _rabbitMQClient;

        public MessageController(IRabbitMQClient rabbitMQClient)
        {
            _rabbitMQClient = rabbitMQClient;
        }

        [HttpPost]
        public IActionResult SendMessage(string message)
        {
            _rabbitMQClient.PublishMessage("my_queue", message);

            return Ok("Сообщение отправлено");
        }
    }
}
