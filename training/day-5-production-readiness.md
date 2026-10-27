# Day 5 — Production readiness and capstone

## Review checklist
- [ ] Clear service responsibilities and API contracts.
- [ ] Input validation and consistent error status codes.
- [ ] Timeouts and controlled dependency failure behavior.
- [ ] Useful logs without secrets or unnecessary candidate personal data.
- [ ] Readiness checks reflect dependencies where appropriate.
- [ ] Secrets injected at runtime, never committed.
- [ ] Durable storage and migrations planned.
- [ ] Authentication and authorization for protected operations.
- [ ] Integration tests for success and failure cases.
- [ ] Metrics, traces, alerts, and rollback plan.

## Capstone (pairs)
Extend the system with a registration status or exam-session service:
1. Draw the service boundary and define the API contract.
2. Implement validation and one failure path.
3. Route the endpoint through Gateway.
4. Update Compose configuration.
5. Demonstrate normal operation and one failure scenario.
6. Present tests and production trade-offs.

## Assessment (20 points)
- Design and boundaries: 4
- API and validation: 4
- Inter-service failure handling: 4
- Docker reproducibility: 4
- Tests and explanation: 4

## Production warning
This sample uses in-memory storage and has no authentication. Do not use it for real candidate data without persistence, security review, access controls, auditability, and monitoring.
