using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderConsumer.Services;
using Testcontainers.Kafka;

public class OrderConsumerIntegrationTests : IAsyncLifetime
{
    private readonly KafkaContainer _kafkaContainer;
    private readonly string _bootstrapServers = "localhost:9093";
    private readonly string _topic = "orders";

    public OrderConsumerIntegrationTests()
    {
        _kafkaContainer = new KafkaBuilder()
            .WithImage("confluentinc/cp-kafka:latest")
            .WithPortBinding(9093, 9093)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _kafkaContainer.StartAsync();
        // Optionally, wait for Kafka to be ready
        await Task.Delay(10000);
    }

    public async Task DisposeAsync()
    {
        await _kafkaContainer.StopAsync();
    }

    [Fact]
    public async Task Consumer_Receives_Produced_Message()
    {
        // Arrange: produce a message
        var producerConfig = new ProducerConfig { BootstrapServers = _bootstrapServers };
        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
        await producer.ProduceAsync(_topic, new Message<string, string> { Key = "test", Value = "test-message" });

        // Arrange: set up consumer service
        var inMemorySettings = new Dictionary<string, string>
        {
            { "Kafka:BootstrapServers", _bootstrapServers },
            { "Kafka:OrderTopic", _topic }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var logger = new LoggerFactory().CreateLogger<OrderConsumerHostedService>();
        var consumerService = new OrderConsumerHostedService(config, logger);

        // Act: start consuming
        consumerService.StartConsuming();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(5000); // Stop after 5 seconds
        await consumerService.StartAsync(cts.Token);

        // Assert: check logs or implement a callback in your service to verify message consumption
        // For a real test, you might inject a callback or use a mock logger to verify the message was processed
    }
}