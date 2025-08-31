using Microsoft.AspNetCore.Mvc;
using OrderConsumer.Services;

namespace OrderConsumer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderConsumerController : ControllerBase
    {
        private readonly OrderConsumerHostedService _consumerService;

        public OrderConsumerController(OrderConsumerHostedService consumerService)
        {
            _consumerService = consumerService;
        }

        [HttpPost("start")]
        public IActionResult StartConsuming()
        {
            _consumerService.StartConsuming();
            return Ok("Started consuming Kafka messages.");
        }

        [HttpPost("stop")]
        public IActionResult StopConsuming()
        {
            _consumerService.StopConsuming();
            return Ok("Stopped consuming Kafka messages.");
        }
    }
}