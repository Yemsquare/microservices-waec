# WAEC .NET Microservices Masterclass

A hands-on .NET 8 training repository for the WAEC engineering team. The original five-day foundation is extended with a durable examination-registration workflow, asynchronous events, Docker infrastructure, automated integration tests, and GitHub Actions CI.

## Architecture

- **ExamGateway** — YARP API gateway, public entry point on port 8080.
- **CatalogService** — starter REST API for subject catalogue concepts.
- **OrdersService** — starter example of synchronous service-to-service HTTP.
- **RegistrationService** — examination registration API, SQL Server persistence with EF Core, validation and duplicate prevention.
- **NotificationWorker** — background consumer of RabbitMQ registration events; logs a demo notification.
- **SQL Server** — durable local development database.
- **RabbitMQ** — message broker with management UI on port 15672.
- **GitHub Actions** — builds all projects and runs Docker-backed integration tests on push and pull request.

The catalog and orders services remain deliberately small for the first labs. The registration service is the WAEC-focused end-to-end example.

## Requirements

Docker Desktop with Docker Compose. A local .NET 8 SDK is required to run the automated integration tests; the service containers build the application projects.

## Run the stack

```bash
docker compose up -d --build
docker compose ps
```

The first SQL Server image startup may take a minute. RegistrationService retries database schema creation while SQL Server starts.

- Gateway health: http://localhost:8080/health
- Subject catalogue: http://localhost:8080/api/catalog/subjects
- Registrations: http://localhost:8080/api/registrations
- RabbitMQ management: http://localhost:15672 (training-only login: waec / training-local-only)

## Create an examination registration

```bash
curl -X POST http://localhost:8080/api/registrations \
  -H "Content-Type: application/json" \
  -d '{"candidateNumber":"WAEC-2027-0001","candidateName":"Ada Okafor","examSession":"2027-May","subjects":["English Language","Mathematics","Biology"]}'
```

A successful request returns HTTP 201. Submitting the same candidate number and exam session again returns HTTP 409. The registration event is published to RabbitMQ and handled asynchronously by the worker.

Watch the worker:

```bash
docker compose logs -f notification-worker
```

## Run automated integration tests

Start the Docker Compose stack first, then run:

```bash
dotnet test tests/Waec.IntegrationTests/Waec.IntegrationTests.csproj --configuration Release
```

The tests cover Catalog CRUD and validation, Orders-to-Catalog communication, Registration persistence and duplicate prevention, RabbitMQ event acknowledgement by the Notification Worker, and Gateway routing.

See [tests/README.md](tests/README.md) for module-by-module coverage, environment variables, and troubleshooting.

## Stop the stack

```bash
docker compose down
```

Remove the local SQL data volume (destructive reset):

```bash
docker compose down -v
```

## Training path

### Core five-day masterclass

1. `training/day-1-foundations.md` — service boundaries and REST.
2. `training/day-2-catalog-service.md` — Minimal APIs and CRUD.
3. `training/day-3-service-communication.md` — typed HTTP client and failure handling.
4. `training/day-4-gateway-and-docker.md` — YARP and Compose.
5. `training/day-5-production-readiness.md` — health, security, and capstone.

### Extension labs

6. `training/day-6-durable-registration-and-messaging.md` — EF Core, SQL Server, RabbitMQ, background worker.
7. `training/day-7-testing-and-ci.md` — module-by-module automated integration tests and CI.

## Learning-only security and reliability notes

- SQL Server and RabbitMQ credentials are local-lab defaults only. Do not expose these ports publicly or reuse these credentials.
- The registration API uses EF Core EnsureCreated to keep the workshop friction low. Replace it with versioned migrations for real deployments.
- Database persistence and message publication are separate operations; the demo can save a registration but fail before publishing the event. A transactional outbox is a follow-on exercise.
- The worker logs a notification rather than sending email/SMS. Integrate an approved provider only after secrets management, retry policy, privacy review, and delivery monitoring are addressed.
- Integration tests expect the Docker Compose services and RabbitMQ management plugin to be available locally. GitHub Actions starts and stops an isolated Compose stack for CI.

## Repository layout

- `src/` — service and worker code with explanatory comments.
- `tests/` — module-by-module automated integration tests.
- `training/` — facilitator-ready day-by-day notes and extension labs.
- `.github/workflows/dotnet.yml` — build and integration-test workflow.
- `docker-compose.yml` — local infrastructure and service wiring.
