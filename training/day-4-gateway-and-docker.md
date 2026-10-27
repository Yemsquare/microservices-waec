# Day 4 — API Gateway and Docker

## Concepts
Reverse proxy, route matching, multi-stage builds, Compose networking, environment configuration.

## Lab
- Inspect `src/ExamGateway/appsettings.json` and identify route-to-cluster mappings.
- Review each service Dockerfile.
- Run `docker compose up --build`, then `docker compose ps` and `docker compose logs -f orders`.
- Explain why Orders calls `http://catalog:8080` inside Compose instead of `localhost:8081`.
- Compare gateway port 8080 with direct service ports 8081 and 8082.

## Challenge
Add a new route, override a destination through an environment variable, and rebuild only the changed service.

## Deliverable
A running Compose stack and an explanation of internal vs host port mapping.
