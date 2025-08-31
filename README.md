# MicroservicesWithKafka

This solution demonstrates a simple microservices architecture using .NET 8 and Apache Kafka for messaging. It includes two main services:

- **OrderProducer**: Publishes order messages to a Kafka topic.
- **OrderConsumer**: Consumes order messages from the Kafka topic.

## Projects

### OrderProducer

- ASP.NET Core Web API
- Exposes an endpoint to produce orders to Kafka.
- Uses `Confluent.Kafka` for Kafka integration.

#### Key Files

- `Controllers/OrderController.cs`: API endpoint to publish orders.
- `Services/OrderProducer.cs`: Kafka producer logic.
- `Models/OrderRequest.cs`: Order data model.
- `appsettings.json`: Kafka configuration (`BootstrapServers`, `OrderTopic`).

#### Example API Request
POST /api/order Content-Type: application/json
{ "OrderId": 123, "ProductName": "Widget", "Quantity": 2, "Price": 19.99 }

### OrderConsumer

- ASP.NET Core Web API
- Implements a hosted service to consume messages from Kafka.
- Consumption can be started/stopped via API endpoints.
- Uses `Confluent.Kafka` for Kafka integration.

#### Key Files

- `Services/OrderConsumerHostedService.cs`: Background Kafka consumer.
- `Controllers/OrderConsumerController.cs`: API to start/stop consuming.
- `appsettings.json`: Kafka configuration (`BootstrapServers`, `OrderTopic`).

#### Example API Usage

- `POST /api/orderconsumer/start` — Start consuming messages.
- `POST /api/orderconsumer/stop` — Stop consuming messages.

## Kafka Configuration

Both services use the following settings in `appsettings.json`:
"Kafka": { "BootstrapServers": "localhost:9092", "OrderTopic": "orders" }

## Running Locally

1. **Start Kafka**  
   Ensure a Kafka broker is running and accessible at the configured `BootstrapServers`.

2. **Run OrderProducer**  
   - Start the API.
   - Use Swagger, Postman, or HTTP client to POST orders.

3. **Run OrderConsumer**  
   - Start the API.
   - Use the controller endpoints to start/stop consuming.

## Testing

### 1. Integration Testing with Test Kafka Broker
- Recommended for real-world validation.
- Use a test Kafka broker (local, Docker, or in-memory) and send requests to your API.
- After posting to `/api/order`, consume messages from the Kafka topic to verify they were produced.

**Example using Testcontainers and xUnit:**
- Spin up a Kafka container for tests.
- Use `HttpClient` to POST to your API.
- Use a Kafka consumer to check the message.

---

### 2. Unit Testing with Mocking
- Recommended for fast, isolated tests.
- Mock the `OrderProducer` service using a library like Moq.
- Verify that `ProduceAsync` is called with the correct parameters.

**Example (xUnit + Moq):**
- Use Moq to mock dependencies and verify method calls.

---

### 3. Manual Testing with Swagger or HTTP Client
- Run your application.
- Use Swagger UI or a tool like Postman to POST to `/api/order`.
- Check your Kafka topic (using a Kafka consumer or CLI) to verify the message.

---

### 4. End-to-End Testing
- Combine API and Kafka checks.
- Use a real or test Kafka broker.
- POST to the API and consume from Kafka to verify the full flow.

---

**Summary:**
- For quick feedback, use unit tests with mocking.
- For real Kafka validation, use integration or end-to-end tests with a test broker.
- Manual testing is useful for initial verification.

You can POST to `/api/orderconsumer/start` or `/api/orderconsumer/stop` to control Kafka consumption.

## License

MIT
