# AuthService — JWT and Role-Based Access Control

AuthService owns user credentials and roles. It issues signed JWT access tokens; the gateway validates those tokens and enforces route policies.

## Roles
- **Administrator** — creates staff accounts and has full access.
- **Examiner** — manages catalogue content and reads candidate registrations.
- **Candidate** — self-registers and submits examination registrations.

Public registration always assigns Candidate. A caller cannot submit a role to elevate their privileges.

## Endpoints
- `POST /api/auth/register` — { "email": "...", "password": "..." }
- `POST /api/auth/login` — returns a Bearer access token
- `GET /api/auth/me` — current token identity
- `POST /api/auth/users` — Administrator only; provisions Examiner or Administrator
- `GET /health`

## Local security configuration
Create the repository-root `.env` from `.env.example`. Configure `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`, `ADMIN_EMAIL`, and `ADMIN_PASSWORD` there. Docker Compose injects the same JWT signing key, issuer, and audience into AuthService and ExamGateway. Use a cryptographically random JWT key of at least 32 bytes and a strong unique bootstrap administrator password. Never commit the real `.env` or put secrets in appsettings files. Rotate credentials that may have been committed previously.

This training service uses Identity password hashing and SQL Server. It uses `EnsureCreated` to reduce workshop setup; production should use migrations, secret management, MFA, lockout policy, refresh-token rotation, audit logging, rate limiting, and key rotation.
