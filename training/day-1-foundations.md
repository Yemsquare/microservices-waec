# Day 1 — Foundations and service boundaries

## Outcomes
- Explain monolith vs microservices and the trade-offs.
- Identify ownership boundaries and public APIs.
- Run the starter system and trace requests.

## Instructor walkthrough
1. Draw a monolith containing Catalog and Orders modules.
2. Draw the target: client → Gateway → Catalog / Orders.
3. Discuss independent deployment, bounded contexts, network failures, and operational overhead.
4. Inspect the projects and Dockerfiles.
5. Run `docker compose up --build`.

## Lab
- Open `http://localhost:8080/health`.
- Call `GET /api/catalog/subjects` and `GET /api/orders`.
- Identify which container responds to each call.
- Change a health response and rebuild.

## Discuss
Why should Orders ask Catalog to validate a subject instead of accessing Catalog storage? When is a modular monolith preferable?

## Deliverable
A diagram listing each service's responsibility and public endpoints.
