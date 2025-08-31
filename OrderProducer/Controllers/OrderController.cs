using Microsoft.AspNetCore.Mvc;
using OrderProducer.Models;
using System.Text.Json;

namespace OrderProducer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly Services.OrderProducer _orderProducer; 

        private readonly IConfiguration _configuration;

        public OrderController(Services.OrderProducer orderProducer, IConfiguration configuration) 
        {
            _orderProducer = orderProducer;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> ProduceOrder([FromBody] OrderRequest orderRequest)
        {
            if (orderRequest == null || orderRequest.OrderId == 0)
            {
                return BadRequest("OrderId is required.");
            }

            var topic = _configuration["Kafka:OrderTopic"] ?? "orders";
            var message = JsonSerializer.Serialize(orderRequest);

            await _orderProducer.ProduceAsync(topic, orderRequest.OrderId.ToString(), message);

            return Ok(new { Status = "Order produced", OrderId = orderRequest.OrderId });
        }
    }
}