# WAEC .NET Microservices Masterclass

A five-day hands-on .NET 8 training project for the WAEC engineering team.

## Build
- **ExamGateway** — YARP API gateway.
- **CatalogService** — subject catalog REST API.
- **OrdersService** — registration orders and synchronous Catalog HTTP calls.
- **Docker Compose** — runs the complete stack without installing .NET locally.

> Educational scaffold: in-memory storage resets on restart. Production needs durable storage, authentication, validation, resilience, secrets management, observability, and automated tests.

## Run
```bash
docker compose up --build
```
Open http://localhost:8080/health, http://localhost:8080/api/catalog/subjects, and http://localhost:8080/api/orders.

Create an order:
```bash
curl -X POST http://localhost:8080/api/orders -H "Content-Type: application/json" -d '{"subjectId":1,"candidateName":"Ada Okafor","examSession":"2027-May"}'
```
Stop: `docker compose down`.

## Five-day modules
1. `training/day-1-foundations.md` — service boundaries and REST.
2. `training/day-2-catalog-service.md` — Minimal APIs and CRUD.
3. `training/day-3-service-communication.md` — typed HTTP client and failures.
4. `training/day-4-gateway-and-docker.md` — YARP and Compose.
5. `training/day-5-production-readiness.md` — health, security, tests, capstone.

Services live in `src/ExamGateway`, `src/CatalogService`, and `src/OrdersService`. Each source file includes explanatory comments. See training notes for guided labs and deliverables.