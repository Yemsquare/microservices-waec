# Day 2 — Catalog REST API

## Concepts
Minimal APIs, request DTOs, route parameters, dependency injection, HTTP status codes, in-memory repositories.

## Guided lab
1. Read subjects: `GET /api/catalog/subjects`.
2. Read one: `GET /api/catalog/subjects/1`.
3. Add Physics with POST body `{"code":"PHY","name":"Physics","active":true}`.
4. Update an item with PUT and delete an item with DELETE.
5. Try empty input and an unknown ID; record the status codes.

## Instructor prompts
- Why use request records instead of binding storage entities directly?
- Explain 400, 404, 201, and 204.
- What happens to data after a container restarts?

## Extension
Reject duplicate subject codes with `409 Conflict` and add a filter for active subjects.

## Deliverable
An API contract table with method, path, purpose, and response codes.
