using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using OrderProducer.Controllers;
using OrderProducer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderProducer.Tests
{
    public class OrderControllerTests
    {
        [Fact]
        public async Task ProduceOrder_ValidOrder_ReturnsOk()
        {
            // Arrange
            var mockProducer = new Mock<OrderProducer.Services.OrderProducer>(null, null);
            mockProducer
                .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var inMemorySettings = new Dictionary<string, string> { { "Kafka:OrderTopic", "orders" } };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var controller = new OrderController(mockProducer.Object, config);

            var orderRequest = new OrderRequest
            {
                OrderId = 123,
                ProductName = "TestProduct",
                Quantity = 1,
                Price = 9.99m
            };

            // Act
            var result = await controller.ProduceOrder(orderRequest);

            // Assert
            mockProducer.Verify(p => p.ProduceAsync("orders", "123", It.IsAny<string>()), Times.Once);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ProduceOrder_NullOrder_ReturnsBadRequest()
        {
            // Arrange
            var mockProducer = new Mock<OrderProducer.Services.OrderProducer>(null, null);
            var config = new ConfigurationBuilder().Build();
            var controller = new OrderController(mockProducer.Object, config);

            // Act
            var result = await controller.ProduceOrder(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}