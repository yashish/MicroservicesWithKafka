# OrderConsumer.Tests

This project contains unit and integration tests for the OrderConsumer microservice.

## Testing Strategies

### 1. Integration Testing with Testcontainers

- **Recommended for real-world validation.**
- Uses [Testcontainers](https://github.com/testcontainers/testcontainers-dotnet) to spin up a Kafka broker in a Docker container for tests.
- After producing a message to Kafka, the consumer service is started and its ability to consume messages is verified.

**Example Flow:**
- Spin up a Kafka container for tests.
- Use `Confluent.Kafka` producer to POST a message to the topic.
- Start the consumer service and verify it processes the message.

**Sample Test Snippet:**
```csharp
var kafkaContainer = new TestcontainersBuilder<TestcontainersContainer>() 
	.WithImage("confluentinc/cp-kafka:latest") 
	.WithEnvironment("KAFKA_ADVERTISED_LISTENERS", "PLAINTEXT://localhost:9093") 
	.WithEnvironment("KAFKA_LISTENERS", "PLAINTEXT://0.0.0.0:9093") 
	.WithEnvironment("KAFKA_BROKER_ID", "1") 
	.WithEnvironment("KAFKA_ZOOKEEPER_CONNECT", "localhost:2181") 
	.WithPortBinding(9093, 9093) 
	.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(9093)) 
	.Build();

await kafkaContainer.StartAsync(); 
await Task.Delay(10000); // Wait for Kafka to be ready

var producerConfig = new ProducerConfig { BootstrapServers = "localhost:9093" }; 
using var producer = new ProducerBuilder<string, string>(producerConfig).Build(); 

await producer.ProduceAsync("orders", new Message<string, string> { Key = "test", Value = "test-message" });
```

### 2. Unit Testing with Mocking

- **Recommended for fast, isolated tests.**
- Uses [Moq](https://github.com/moq/moq) to mock dependencies such as `ILogger` and `IConfiguration`.
- Verifies that methods like `StartConsuming`, `StopConsuming`, and error handling logic work as expected.

**Sample Test Snippet:**
```csharp
var mockLogger = new Mock<ILogger<OrderConsumerHostedService>>();
var mockConfig = new Mock<IConfiguration>();
var consumer = new OrderConsumerHostedService(mockLogger.Object, mockConfig.Object);

// Act
consumer.StartConsuming();
//await Task.Delay(1000); // Wait for some time to simulate consumption
service.StopConsuming();

// Assert
mockLogger.Verify(logger => logger.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce());
```

### 3. Manual Testing

- Run your application and use Swagger UI or Postman to POST messages to Kafka.
- Use the consumer service to verify messages are consumed.

---

## How to Run Tests

- **Unit Tests:**  
  Run with Visual Studio Test Explorer or:
```bash
dotnet test OrderConsumer.Tests/OrderConsumer.Tests.csproj
```

- **Integration Tests:**  
Ensure Docker is running on your machine. The integration tests will automatically start and stop a Kafka container.

---

## Dependencies

- [DotNet.Testcontainers](https://github.com/testcontainers/testcontainers-dotnet)
- [Confluent.Kafka](https://github.com/confluentinc/confluent-kafka-dotnet)
- [xUnit](https://xunit.net/)
- [Moq](https://github.com/moq/moq)

---

## Notes

- Integration tests may take longer to run due to container startup time.
- You can extend integration tests to verify message consumption by inspecting logs or adding hooks/callbacks in your consumer service.



