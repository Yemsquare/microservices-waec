# Extension Lab: Automated integration tests and CI

This lab adds black-box integration tests for the services already running in Docker Compose. Learners send real HTTP requests to the APIs and verify the responses and cross-service behaviour.

## Prerequisites

- Docker Desktop and Docker Compose are running.
- .NET 8 SDK is installed.
- The repository has been cloned locally.

## Start the training environment

Run from the repository root:

```bash
docker compose up -d --build
docker compose ps
```

Wait until the Gateway, Catalog, Orders, and Registration health endpoints respond:

- http://localhost:8080/health
- http://localhost:8081/health
- http://localhost:8082/health
- http://localhost:8083/health

RabbitMQ management UI: http://localhost:15672

## Run all automated tests

```bash
dotnet test tests/Waec.IntegrationTests/Waec.IntegrationTests.csproj --configuration Release
```

## Test modules and expected behaviour

| Test file | Training module | Assertions |
|---|---|---|
| `CatalogServiceTests.cs` | REST and CRUD | Health, create, read, update, delete, invalid input |
| `OrdersServiceTests.cs` | Synchronous service communication | Orders resolves subject through Catalog; unknown subject and invalid input are rejected |
| `RegistrationServiceTests.cs` | EF Core and SQL Server | Health, create, retrieve, persistence, duplicate prevention, validation, missing ID |
| `RabbitMqMessagingTests.cs` | Asynchronous messaging | Queue has an active consumer and the worker acknowledges a registration event |
| `ExamGatewayTests.cs` | API Gateway and YARP | Gateway health and forwarding to Catalog, Orders, and Registration |

The tests use unique candidate numbers and examination sessions so they can be run repeatedly against the same local SQL Server volume. The shared test collection disables parallel execution because Catalog and Orders use in-memory stores.

The RabbitMQ test reads the broker management API acknowledgement counter. It verifies that a published registration event was consumed and acknowledged; it requires the RabbitMQ management plugin and Notification Worker to be running.

## Run tests against a different host

The test project supports these environment variables:

| Variable | Default |
|---|---|
| `CATALOG_BASE_URL` | `http://localhost:8081/` |
| `ORDERS_BASE_URL` | `http://localhost:8082/` |
| `REGISTRATION_BASE_URL` | `http://localhost:8083/` |
| `GATEWAY_BASE_URL` | `http://localhost:8080/` |
| `RABBITMQ_MANAGEMENT_URL` | `http://localhost:15672/` |
| `RABBITMQ_USER` | `waec` |
| `RABBITMQ_PASSWORD` | `training-local-only` |

## GitHub Actions

The workflow at `.github/workflows/dotnet.yml` builds each service, starts the Docker Compose stack, waits for health endpoints, runs the integration suite, and uploads the TRX test report and container logs as workflow artifacts.

## Learner tasks

1. Add a test for updating a subject with invalid data.
2. Add a test for retrieving an order by ID.
3. Add a test proving that a registration remains in SQL Server after Registration Service restarts.
4. Stop Catalog and verify that Orders returns HTTP 503 for a valid-looking order.
5. Explain why an HTTP 201 from Registration Service does not, by itself, prove that the Notification Worker processed the event.

## Discussion: reliability boundary

Registration Service currently saves the record and then publishes to RabbitMQ. These are two separate operations. If the database save succeeds but publishing fails, the record remains saved even though the request may fail. The Transactional Outbox Pattern is the next advanced exercise.

## Stop the environment

```bash
docker compose down
```

Do not use `docker compose down -v` on a learner's machine unless the learner intentionally wants to delete the SQL Server data volume.
