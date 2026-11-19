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
Set `Jwt__Key` to a random secret of at least 32 UTF-8 bytes and use the exact same key, issuer, and audience in AuthService and ExamGateway. Set `BootstrapAdmin__Email` and `BootstrapAdmin__Password` to provision the first administrator. Never commit production secrets.

This training service uses Identity password hashing and SQL Server. It uses `EnsureCreated` to reduce workshop setup; production should use migrations, secret management, MFA, lockout policy, refresh-token rotation, audit logging, rate limiting, and key rotation.


the repo structure
AuthService/
├── Program.cs
├── Common/            Result.cs, ResultExtensions.cs
├── Options/           JwtOptions.cs, BootstrapAdminOptions.cs
├── Security/          Roles.cs, Permissions.cs, TokenHasher.cs
├── Entities/          RefreshToken.cs
├── Data/              AuthDbContext.cs, AuthDbInitializer.cs
├── DTOs/              Requests.cs, Responses.cs
├── Repositories/      IRefreshTokenRepository.cs, RefreshTokenRepository.cs
├── Services/          ITokenService.cs, TokenService.cs,
│                      IAuthenticationService.cs, AuthenticationService.cs,
│                      IUserManagementService.cs, UserManagementService.cs
├── Controllers/       AuthController.cs, UsersController.cs
└── Extensions/        ServiceCollectionExtensions.cs


Pseudocode 
Controller      -> validates DTO, calls service, maps Result<T> to HTTP
AuthenticationService
  Register : create user, add Candidate role
  Login    : find user, check password (lockout), tokenService.IssueTokenPair
  Refresh  : begin tx, load token by hash, check active + not locked out,
             revoke old, issue new pair, set ReplacedBy, save, commit
             (DbUpdateConcurrencyException -> Unauthorized)
  Logout   : revoke token if active
TokenService    -> builds claims (roles + permission claims), signs JWT,
                   generates random refresh token, stores only its hash
UserManagementService -> list, create staff, change role, enable/disable
Repository      -> only RefreshToken persistence
Initializer     -> EnsureCreated with retry, seed roles + permission claims, bootstrap admin