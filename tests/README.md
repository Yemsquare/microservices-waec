# WAEC microservices automated integration tests

These tests exercise the running Docker Compose stack over HTTP. They are intentionally black-box tests: learners test the APIs and service boundaries without coupling tests to internal implementation classes.

## 1. Start the services

From the repository root:

```bash
docker compose up -d --build
docker compose ps
```

Wait until the containers are healthy/running. Registration Service also waits for SQL Server schema initialization.

## 2. Run the tests

Install the .NET 8 SDK, then run:

```bash
dotnet test tests/Waec.IntegrationTests/Waec.IntegrationTests.csproj --configuration Release
```

The test project defaults to these local endpoints:

| Environment variable | Default |
|---|---|
| `CATALOG_BASE_URL` | `http://localhost:8081/` |
| `ORDERS_BASE_URL` | `http://localhost:8082/` |
| `REGISTRATION_BASE_URL` | `http://localhost:8083/` |
| `GATEWAY_BASE_URL` | `http://localhost:8080/` |
| `RABBITMQ_MANAGEMENT_URL` | `http://localhost:15672/` |
| `RABBITMQ_USER` | `waec` |
| `RABBITMQ_PASSWORD` | `training-local-only` |

Override any URL when running the tests against a different host or port.

## 3. Test coverage by training module

| Test file | Module | What it verifies |
|---|---|---|
| `CatalogServiceTests.cs` | REST APIs / CRUD | Health, create, read, update, delete, validation |
| `OrdersServiceTests.cs` | Synchronous service communication | Orders calls Catalog, valid subject, unknown subject, request validation |
| `RegistrationServiceTests.cs` | EF Core / SQL Server | Health, persistence, retrieval, duplicate prevention, validation, missing IDs |
| `RabbitMqMessagingTests.cs` | Event-driven communication | Queue has an active consumer and a registration event is acknowledged |
| `ExamGatewayTests.cs` | API Gateway / YARP | Gateway health and forwarding to Catalog, Orders, and Registration |

The RabbitMQ test uses the management API's acknowledgement counter to verify that the Notification Worker consumed the event. It therefore requires the RabbitMQ management plugin and the worker to be running.

## 4. Stop the stack

```bash
docker compose down
```

Do not add `-v` unless you intentionally want to delete the local SQL Server data volume.

## 5. Run in CI

GitHub Actions starts the Compose stack, waits for the health endpoints, runs this test project, saves the TRX result, prints service logs on failure, and shuts the stack down.
