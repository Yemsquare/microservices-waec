# Extension Lab: Durable registrations and asynchronous events

This optional lab extends the five-day foundation with a WAEC examination-registration workflow.

## Outcomes
- Persist registrations in SQL Server with EF Core.
- Enforce candidate/session uniqueness in the database.
- Publish a RegistrationCreatedEvent to RabbitMQ.
- Consume the event in a separate worker.
- Compare synchronous HTTP calls with asynchronous messaging.

## Architecture
Client -> ExamGateway -> RegistrationService -> SQL Server
RegistrationService -> RabbitMQ -> NotificationWorker (logs a demo notification)

## Run and create a registration
```bash
docker compose up --build
curl -X POST http://localhost:8080/api/registrations \
  -H "Content-Type: application/json" \
  -d '{"candidateNumber":"WAEC-2027-0001","candidateName":"Ada Okafor","examSession":"2027-May","subjects":["English Language","Mathematics","Biology"]}'
curl http://localhost:8080/api/registrations
docker compose logs -f notification-worker
```

## Exercises
1. Submit the same candidate number and exam session twice; observe HTTP 409.
2. Stop the worker, create a registration, restart the worker, and observe queued delivery.
3. Add a GET endpoint filtered by candidate number.
4. Explain the failure window between saving SQL data and publishing the event. Propose a transactional outbox.
5. Add retry/backoff and dead-letter handling.

## Facilitator notes
- EnsureCreated is only for the disposable workshop; use versioned EF migrations in production.
- The sample publisher opens a connection for each event for simplicity. Production should reuse connections safely.
- Database save and broker publish are not atomic; use an outbox and idempotent consumer for reliable processing.
- Default credentials are local-lab values only.
