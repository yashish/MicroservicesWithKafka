using Confluent.Kafka;
using OrderConsumer.Models;
using System.Text.Json;

namespace OrderConsumer.Services
{
    public class OrderConsumerHostedService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderConsumerHostedService> _logger;
        private readonly string _topic;
        private readonly string _bootstrapServers;
        private bool _shouldConsume = false;

        public OrderConsumerHostedService(IConfiguration configuration, ILogger<OrderConsumerHostedService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _topic = _configuration["Kafka:OrderTopic"] ?? "orders";
            _bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        }

        public void StartConsuming()
        {
            _shouldConsume = true;
        }

        public void StopConsuming()
        {
            _shouldConsume = false;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "order-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest //start reading all messages from the beginning if no offset is committed in the topic partition (useful for analyzing all messages)
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetErrorHandler((_, e) => _logger.LogError("Kafka Consumer error: {Reason}", e.Reason))
                .SetLogHandler((_, logMessage) => _logger.LogInformation("Kafka log: {Message}", logMessage.Message))
                .Build();

            consumer.Subscribe(_topic);

            _logger.LogInformation("OrderConsumerHostedService started. Waiting for StartConsuming...");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_shouldConsume)
                {
                    try
                    {
                        var result = consumer.Consume(stoppingToken);
                        if (result != null)
                        {
                            try
                            {
                                // Example: Deserialize message value
                                 var order = JsonSerializer.Deserialize<OrderRequest>(result.Message.Value);
                                _logger.LogInformation("Received message: Key={Key}, Value={Value}, Partition={Partition}, Offset={Offset}",
                                    result.Message.Key, result.Message.Value, result.Partition, result.Offset);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error deserializing message: {Value}", result.Message.Value);
                                // Optionally: send to dead-letter topic or skip
                            }
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Kafka consumption error: {Reason}", ex.Error.Reason);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Consumer cancellation requested.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error during consumption.");
                    }
                }
                else
                {
                    await Task.Delay(500, stoppingToken);
                }
            }

            consumer.Close();
            _logger.LogInformation("Consumer closed.");
        }
    }
}