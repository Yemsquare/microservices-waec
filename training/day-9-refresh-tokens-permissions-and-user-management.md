# Day 9 — Refresh Tokens, Permissions, and User Administration

## Learning outcomes
Learners will implement refresh-token rotation, logout/revocation, permission claims, policy-based authorization, and administrator user-management operations.

## Why access and refresh tokens are different
An access token is short-lived (10 minutes in this lab) and is sent to APIs. A refresh token is a long, random opaque value used only to obtain a new access token. The API stores only a SHA-256 hash of the refresh token, never the raw token.

A refresh token is single-use:
1. Client sends the refresh token to `POST /api/auth/refresh`.
2. AuthService hashes it and finds the matching database record.
3. AuthService rejects missing, expired, revoked, or disabled-user sessions.
4. AuthService revokes the old token and creates a new token pair in one database transaction.
5. Replaying the old token returns 401.

Logout revokes the current refresh token. The already-issued access token remains usable until its short expiry; this is why access tokens must be short-lived.

## Permission-based authorization
Roles group permissions. The API checks permissions rather than depending directly on role names.

| Role | Permission claims |
|---|---|
| Candidate | `catalog.read`, `registrations.create`, `profile.read` |
| Examiner | Candidate permissions plus `catalog.manage`, `registrations.read` |
| Administrator | All permissions plus `users.manage` |

The AuthService copies the current role permissions into the access token. The YARP gateway checks these claims using named policies such as `CatalogManage` and `RegistrationsRead`.

## Administrator user-management API

All endpoints below require the `users.manage` permission.

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/auth/users` | List users and their roles/status |
| POST | `/api/auth/users` | Provision an Examiner or Administrator |
| PUT | `/api/auth/users/{userId}/role` | Change a user's role |
| PUT | `/api/auth/users/{userId}/status` | Enable or disable a user |

Example role change:

```bash
curl -X PUT http://localhost:8080/api/auth/users/USER_ID/role \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"role":"Examiner"}'
```

Example disable:

```bash
curl -X PUT http://localhost:8080/api/auth/users/USER_ID/status \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"enabled":false}'
```

## Practical lab — test refresh-token rotation

```bash
# Login and capture both tokens.
LOGIN=$(curl -s -X POST http://localhost:8080/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"candidate@example.com","password":"Candidate#2026Pass"}')
ACCESS_TOKEN=$(echo "$LOGIN" | jq -r .accessToken)
REFRESH_TOKEN=$(echo "$LOGIN" | jq -r .refreshToken)

# Rotate the refresh token. Save the returned replacement immediately.
ROTATED=$(curl -s -X POST http://localhost:8080/api/auth/refresh \
  -H 'Content-Type: application/json' \
  -d "{\"refreshToken\":\"$REFRESH_TOKEN\"}")
NEW_REFRESH_TOKEN=$(echo "$ROTATED" | jq -r .refreshToken)

# Replaying the old token must return 401 Unauthorized.
curl -i -X POST http://localhost:8080/api/auth/refresh \
  -H 'Content-Type: application/json' \
  -d "{\"refreshToken\":\"$REFRESH_TOKEN\"}"

# Logout revokes the latest refresh token.
curl -i -X POST http://localhost:8080/api/auth/logout \
  -H 'Content-Type: application/json' \
  -d "{\"refreshToken\":\"$NEW_REFRESH_TOKEN\"}"
```

## Instructor code walkthrough
- `RefreshToken` stores the token hash, user, expiry, revocation time, replacement hash, and SQL Server rowversion.
- `HashToken` uses SHA-256 to ensure a database read does not reveal usable refresh credentials.
- `IssueTokenPairAsync` creates access and refresh tokens in one shared path for login and refresh.
- Role claims are read from ASP.NET Core Identity and emitted as `permission` JWT claims.
- Gateway authorization policies use `RequireClaim`, separating endpoint capability from the name of the role.
- Admin endpoints return safe user summaries, never password hashes or security stamps.

## Security discussion
- Refresh tokens should be kept in secure, HTTP-only, same-site cookies for browser clients; do not store long-lived tokens in localStorage.
- Mobile clients should use OS-protected secure storage.
- Use HTTPS everywhere and redact tokens from logs.
- A refresh token is a credential: protect it like a password.
- The database rowversion detects concurrent attempts to rotate one refresh token. Handle concurrency conflicts as an unauthorized/replay response in production.
- Disabling an account prevents new login and refresh. Existing access tokens remain valid until expiry unless a token-revocation/introspection mechanism is added.
- Role changes affect newly issued tokens. With a 10-minute access-token lifetime, old permissions can remain active for at most that window.
- The sample uses a symmetric signing key for teaching. Prefer OpenID Connect with an established identity provider or asymmetric signing keys in production.

## Exercises
1. Add a `results.publish` permission for Examiners and Administrators.
2. Protect a new results endpoint at the gateway using a permission policy.
3. Add a test proving that a Candidate receives 403.
4. Add refresh-token replay handling that revokes every session belonging to the user when a revoked token is replayed.
5. Add audit events for role changes, account disablement, login failures, and logout.
