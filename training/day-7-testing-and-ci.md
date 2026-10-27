# Extension Lab: Automated checks and CI

## Tasks
1. Add xUnit and Microsoft.AspNetCore.Mvc.Testing test projects.
2. Test validation, successful registration, duplicate candidate/session, and missing IDs.
3. Add an integration test that exercises the gateway and registration API.
4. Add a GitHub Actions workflow that restores, builds, and runs tests.
5. Save test results as workflow artifacts.

## Suggested cases
- Missing candidate number returns HTTP 400.
- A valid registration returns HTTP 201 and a Location header.
- A repeated candidate/session returns HTTP 409.
- Registration-created events eventually appear in worker logs.
- If RabbitMQ is unavailable after the database commit, document the consistency gap and implement an outbox as an advanced exercise.

## Discussion
Tests should not depend on a developer's machine state. Use isolated test databases, deterministic fixtures, readiness checks, and a separate integration-test stage for Docker services.
