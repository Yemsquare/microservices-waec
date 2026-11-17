# Day 8 — Authentication Service and Role-Based Access Control

## Learning outcomes
By the end of this lab, learners can explain authentication versus authorization, issue and validate JWT access tokens, understand claims and roles, protect YARP routes with policies, and test 401 versus 403 responses.

## Architecture and request flow
1. A Candidate registers at `POST /api/auth/register`. AuthService hashes the password with ASP.NET Core Identity and assigns only the Candidate role.
2. The user logs in at `POST /api/auth/login`. AuthService verifies the password and issues a signed JWT containing subject, email, expiry, and role claims.
3. The client sends `Authorization: Bearer <token>` to ExamGateway.
4. ExamGateway validates the signature, issuer, audience, and expiry. YARP forwards only requests allowed by the route's authorization policy.
5. A valid token without the required role receives 403 Forbidden; no or invalid token receives 401 Unauthorized.

## Role matrix

| Capability | Candidate | Examiner | Administrator |
|---|---:|---:|---:|
| Register/login | Yes | Yes | Yes |
| Read catalogue | Yes | Yes | Yes |
| Create/update/delete subjects | No | Yes | Yes |
| Create registration | Yes | Yes | Yes |
| Read registrations | No | Yes | Yes |
| Provision staff accounts | No | No | Yes |

## Start and configure
The Docker Compose lab uses local-only defaults for convenience. Before running, replace `Jwt__Key`, `BootstrapAdmin__Email`, and `BootstrapAdmin__Password` in your local environment. Keep the same JWT key, issuer, and audience in AuthService and ExamGateway. Do not use workshop defaults on a public server.

```bash
export JWT_KEY='replace-with-a-random-secret-at-least-32-bytes-long'
export ADMIN_EMAIL='admin@waec-training.local'
export ADMIN_PASSWORD='Replace-With-A-Strong-Password-2026!'
docker compose up -d --build
```

## Test the flow

### 1. Register a candidate
```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H 'Content-Type: application/json' \
  -d '{"email":"candidate@example.com","password":"Candidate#2026Pass"}'
```

### 2. Login and capture the token
```bash
TOKEN=$(curl -s -X POST http://localhost:8080/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"candidate@example.com","password":"Candidate#2026Pass"}' | jq -r .accessToken)
```

### 3. Check identity and permitted access
```bash
curl http://localhost:8080/api/auth/me -H "Authorization: Bearer $TOKEN"
curl http://localhost:8080/api/catalog/subjects -H "Authorization: Bearer $TOKEN"
```

### 4. Confirm role enforcement
A Candidate may create a registration, but cannot read the full registrations list or modify the subject catalogue.

```bash
# Candidate can submit a registration (expected 201 for a new candidate/session).
curl -i -X POST http://localhost:8080/api/registrations \
  -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d '{"candidateNumber":"WAEC-2027-0002","candidateName":"Ada Okafor","examSession":"2027-May","subjects":["English Language","Mathematics"]}'

# Candidate cannot read all registrations (expected 403).
curl -i http://localhost:8080/api/registrations \
  -H "Authorization: Bearer $TOKEN"

# Candidate cannot create a subject (expected 403).
curl -i -X POST http://localhost:8080/api/catalog/subjects \
  -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d '{"code":"PHY","name":"Physics"}'
```

### 5. Provision an Examiner
Login as the bootstrap Administrator, then use its token to call `POST /api/auth/users` with an email, temporary password, and role `Examiner`. This endpoint is protected by the Administrator policy. The new Examiner can read registrations and manage subjects.

## Instructor discussion
- Authentication answers “Who are you?”; authorization answers “What may you do?”
- JWTs are signed, not encrypted. Do not put passwords or secrets in claims.
- Role claims are copied into the token at login. Role changes take effect when a new token is issued; short token lifetimes reduce stale authorization.
- 401 means the caller is not authenticated. 403 means authenticated but not authorized.
- Gateway authorization is the perimeter for this workshop. In production, do not publish internal service ports; validate tokens and enforce authorization in each service as defence in depth.
- Use an asymmetric signing key or managed identity provider in production; rotate keys and avoid sharing a symmetric key across independently administered services.

## Practical assignment
Add a `Chief Examiner` policy, protect one new route, write tests for anonymous, Candidate, Examiner, and Administrator callers, and demonstrate 401 and 403 outcomes.


## Extension: permission claims and session lifecycle

The advanced implementation now includes:
- 10-minute JWT access tokens and 7-day opaque refresh tokens.
- Refresh-token rotation at `POST /api/auth/refresh`.
- Logout/revocation at `POST /api/auth/logout`.
- Permission claims grouped by role: `catalog.read`, `catalog.manage`, `registrations.read`, `registrations.create`, `profile.read`, and `users.manage`.
- Administrator user listing, role changes, and account enable/disable.

Continue with [Day 9 — Refresh Tokens, Permissions, and User Administration](day-9-refresh-tokens-permissions-and-user-management.md) for the full practical lab.

## Code-commenting standard

The source code deliberately includes comments explaining why each security step exists, not merely repeating what the next line does. Ask learners to explain the threat each comment addresses: privilege escalation, token theft, refresh replay, or stale authorization.
