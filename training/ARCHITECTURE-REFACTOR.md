# WAEC Microservices – Refactored Structure

This branch separates service startup from endpoint modules, data access, business stores, contracts, and messaging.

- AuthService: Identity, JWT and refresh-token endpoints, DTOs, entities, EF Core context, role seeding, and dependency registration.
- CatalogService: endpoint mappings, subject models, and subject store.
- OrdersService: endpoint mappings, order store/models, and typed Catalog HTTP client.
- RegistrationService: endpoint mappings, DTOs, EF Core context/entities, database initializer, event contract, and RabbitMQ publisher.
- ExamGateway: JWT validation, authorization policies, Swagger aggregation, and YARP registration.
- NotificationWorker: hosted consumer and event contract.

The project continues to use Minimal APIs, so route definitions live in focused endpoint classes instead of MVC controllers. This keeps the current API style and separates HTTP routing from the startup composition root without introducing an unnecessary MVC conversion.

The Catalog and Orders stores remain in-memory as in the original training implementation. Auth and Registration retain SQL Server ownership and the workshop EnsureCreated approach. The Registration publisher still has the publish-after-save dual-write gap; a transactional outbox is a separate production-hardening task.
