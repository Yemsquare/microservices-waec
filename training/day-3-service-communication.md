# Day 3 — Service-to-service communication

## Concepts
Typed HTTP clients, service ownership, DTO contracts, timeouts, cancellation, dependency failures.

## Trace the flow
1. Client sends POST /api/orders through Gateway.
2. Orders validates required fields.
3. Orders calls Catalog through the typed `CatalogClient`.
4. Orders creates an order only if Catalog confirms the subject exists.

## Lab
- Create an order for subject 1.
- Try subject 999 and observe the validation error.
- Stop Catalog, submit an order, and observe the controlled 503 response and warning log.
- Restart Catalog and retry.

## Discuss
Why do network calls need timeouts? Why is synchronous HTTP different from event-driven messaging? Why can blind retries be dangerous?

## Extension
Add a resilience policy for transient failures. Discuss idempotency before retrying POST requests.

## Deliverable
A sequence diagram showing success and Catalog failure paths.
