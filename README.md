# WAEC .NET Microservices Masterclass

A hands-on .NET 8 training repository for the WAEC engineering team. The project includes an examination-registration workflow, asynchronous events, Docker infrastructure, automated integration tests, CI, and a JWT authentication service with role-based access control.

## Architecture

- **ExamGateway** — YARP API gateway and JWT validation / authorization boundary, public entry point on port 8080.
- **AuthService** — ASP.NET Core Identity, SQL Server user store, password hashing, JWT issuance, rotating refresh tokens, permission claims, and administrator user management (port 8084 for local diagnostics).
- **CatalogService** — REST API for subject catalogue concepts.
- **OrdersService** — synchronous service-to-service HTTP example.
- **RegistrationService** — examination registration API, SQL Server persistence with EF Core, validation and duplicate prevention.
- **NotificationWorker** — RabbitMQ registration-event consumer.
- **SQL Server** — separate `WaecAuth` and `WaecRegistrations` databases.
- **RabbitMQ** — message broker with management UI on port 15672.
- **GitHub Actions** — builds the services and runs Docker-backed integration tests.

## Configure secrets and run the stack

All passwords, database credentials, RabbitMQ credentials, and JWT signing keys are supplied through the root `.env` file. The actual `.env` is ignored by Git; only `.env.example` is committed.

Create your local file:

```bash
cp .env.example .env
```

Edit `.env` and replace every placeholder with a unique strong value. Generate random hexadecimal secrets with `openssl rand -hex 32`. Use separate values for SQL Server, the bootstrap administrator password, RabbitMQ, and the JWT signing key. AuthService and ExamGateway must share the same JWT key, issuer, and audience.

Validate and start the full stack:

```bash
docker compose config
docker compose up -d --build
docker compose ps
```

Stop containers without deleting persistent database data:

```bash
docker compose down
```

Endpoints:
- Gateway health: http://localhost:8080/health
- Student API through gateway: http://localhost:8080/api/students (Bearer token required)
- AuthService health: http://localhost:8084/health
- StudentService Swagger for local training: http://localhost:8085/swagger
- RabbitMQ management: http://localhost:15672 (use credentials from your local `.env`)

## Authentication quick start

Register a candidate (public registration can only assign Candidate):

```bash
curl -X POST http://localhost:8080/api/auth/register -H "Content-Type: application/json" \
  -d '{"email":"candidate@example.com","password":"Candidate#2026Pass"}'
```

Login and capture the token:

```bash
TOKEN=$(curl -s -X POST http://localhost:8080/api/auth/login -H "Content-Type: application/json" \
  -d '{"email":"candidate@example.com","password":"Candidate#2026Pass"}' | jq -r .accessToken)
curl http://localhost:8080/api/auth/me -H "Authorization: Bearer $TOKEN"
```

A Candidate can read the subject catalogue and create a registration. Examiner and Administrator can read registrations and manage the catalogue. Only Administrator has the `users.manage` permission to provision staff, list users, change roles, and disable/enable accounts. Access tokens last 10 minutes by default; refresh tokens are opaque, hashed at rest, single-use, and rotated on refresh.

## Authorization matrix

| Operation | Candidate | Examiner | Administrator |
|---|---:|---:|---:|
| Read subject catalogue | Yes | Yes | Yes |
| Manage subjects | No | Yes | Yes |
| Create registration | Yes | Yes | Yes |
| Read all registrations | No | Yes | Yes |
| Provision staff users | No | No | Yes |

## Tests

Start Compose, then run:

```bash
dotnet test tests/Waec.IntegrationTests/Waec.IntegrationTests.csproj --configuration Release
```

See [tests/README.md](tests/README.md) for the existing module-by-module integration suite. See [training/day-8-authentication-and-rbac.md](training/day-8-authentication-and-rbac.md) for the full instructor lab and curl exercises.

## Training path

1. `training/day-1-foundations.md` — service boundaries and REST.
2. `training/day-2-catalog-service.md` — Minimal APIs and CRUD.
3. `training/day-3-service-communication.md` — typed HTTP client and failure handling.
4. `training/day-4-gateway-and-docker.md` — YARP and Compose.
5. `training/day-5-production-readiness.md` — health, security, and capstone.
6. `training/day-6-durable-registration-and-messaging.md` — EF Core, SQL Server, RabbitMQ, background worker.
7. `training/day-7-testing-and-ci.md` — module-by-module automated integration tests and CI.
8. `training/day-8-authentication-and-rbac.md` — Identity, JWT, claims, roles, and gateway authorization.
9. `training/day-9-refresh-tokens-permissions-and-user-management.md` — refresh-token rotation, permission-based policies, and administrator user management.

## Security and reliability notes

- Gateway is the intended API entry point. Compose still publishes backend ports for local training and existing direct-service integration tests; remove those port mappings and enforce service-level authorization before any public deployment.
- JWTs are signed, not encrypted. Keep secrets out of source control. Use managed secret storage, an OpenID Connect identity provider or asymmetric keys, MFA, lockout, rate limiting, key rotation, audit logging, and short-lived access tokens for production. Browser clients should keep refresh tokens in secure HTTP-only cookies; mobile clients should use OS-protected secure storage.
- The workshop uses `EnsureCreated` for quick setup. Replace it with versioned EF migrations for production.
- SQL Server and RabbitMQ credentials are local-lab defaults only. Do not expose infrastructure ports publicly.
- Registration persistence and event publication are separate operations; a transactional outbox is the production solution to the dual-write gap.


POSTMAN TESTING LINK 
https://aerospace-operator-72165908-s-team.postman.co/workspace/Weac~60ea1d20-e268-4580-adbd-f1e7cbf64fdc/collection/25896085-03717665-9695-46e2-a08b-766a8f930dff?action=share&creator=25896085
## Secrets and deployment safety

- Never commit `.env`; `.env.example` contains placeholders only.
- Rotate credentials that were previously committed. Removing a secret in a later commit does not remove it from Git history.
- Backend ports are published for local training; remove those mappings and enforce service-level authorization before public deployment.
- Do not expose SQL Server or RabbitMQ ports publicly. Use managed secret storage and key rotation in production.
