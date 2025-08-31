using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OrderProducer.Services
{
    public class OrderProducer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderProducer> _logger;
        private readonly IProducer<string, string> _producer;

        public OrderProducer(IConfiguration configuration, ILogger<OrderProducer> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var producerconfig = new ProducerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"]
            };

            _producer = new ProducerBuilder<string, string>(producerconfig)
                .SetErrorHandler((_, e) =>
                {
                    _logger.LogError("Kafka Order ProducerBuilder error: {Reason}", e.Reason);
                    Console.WriteLine($"Kafka Order ProducerBuilder error: {e.Reason}");
                })
                .SetLogHandler((_, logMessage) => _logger.LogInformation("Kafka log: {Message}", logMessage.Message))
                .Build();
        }

        public async Task ProduceAsync(string topic, string orderId, string message)
        {
            var kafkaMessage = new Message<string, string> { Key = orderId, Value = message };

            try
            {
                await _producer.ProduceAsync(topic, kafkaMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while producing Order Kafka message to topic {Topic}", topic);
                Console.WriteLine($"Exception occurred while producing Order Kafka message: {ex.Message}");
                throw; // Optionally rethrow or handle as needed
            }
        }
    }
}
