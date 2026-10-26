# BackendTemplate

A runnable .NET 10 backend starter with a small clean architecture, JWT authentication, permission-based access, and EF Core persistence.

## Run locally

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). To start the API with the Aspire dashboard:

```sh
dotnet restore
dotnet run --project src/BackendTemplate.AppHost
```

Open the dashboard login URL printed in the terminal at `http://localhost:15000`. The `api` resource is available at `http://localhost:5000` and reports its database readiness through `/health/ready`. The dashboard shows its resource status, console output, structured logs, traces, and metrics. The HTTP profile uses loopback addresses and keeps dashboard authentication enabled.

The AppHost runs the existing SQLite-backed API in Development, including automatic migrations and development signing keys. No database container or Aspire CLI/workload installation is needed for this setup. Keep ports 5000 and 15000–15002 free; stop a standalone API before starting the AppHost. Stop the AppHost with Ctrl+C to stop its managed processes.

To run the API on its own:

```sh
dotnet run --project src/BackendTemplate.Api
```

The development profile listens at `http://localhost:5000`. SQLite migrations run automatically in Development and create `src/BackendTemplate.Api/backend.db`. No database server or preconfigured account is needed.

Development generates a temporary signing key when `Jwt:Key` is absent. Restarting invalidates existing tokens. To keep tokens across restarts, set a key of at least 32 UTF-8 bytes through .NET user secrets for `src/BackendTemplate.Api`. Never commit signing keys.

Try the requests in [BackendTemplate.Api.http](src/BackendTemplate.Api/BackendTemplate.Api.http), or register with:

```sh
curl http://localhost:5000/api/auth/register \
  -H 'Content-Type: application/json' \
  -d '{"firstName":"Alice","lastName":"Example","email":"alice@example.com","password":"correct horse battery staple"}'
```

Log in at `POST /api/auth/login` with the same email and password. Send the returned `accessToken` as `Authorization: Bearer <accessToken>` to `GET /api/users/me`.

## API

- `POST /api/auth/register`: public registration; returns 201, a user response, and a Location header. Duplicate normalized emails return 409.
- `POST /api/auth/login`: returns a bearer token and its expiry. Invalid credentials return 401.
- `GET /api/users/me`: returns the authenticated user's profile.
- `GET /api/users/{id}`: allows the account owner or a caller with `users:read`; otherwise returns 403. Authorized reads of missing accounts return 404.
- `GET /health/live`: process liveness.
- `GET /health/ready`: database connectivity.
- `GET /openapi/v1.json`: OpenAPI document, available in Development.

Validation and errors use Problem Details with a trace ID. Registration requires names of up to 100 characters, a valid email, and a password between 12 and 128 characters. Emails are trimmed and normalized to lowercase before lookup and persistence.

## Structure and dependencies

- **Domain**: entities, value objects, and plain domain events; no package dependencies.
- **Application**: commands, queries, asynchronous validation, authorization, and repository/service contracts.
- **Infrastructure**: EF Core, a focused user repository, ASP.NET Core Identity password hashing, and JWT issuance/validation.
- **Api**: the executable host, minimal API routes, Problem Details, rate limiting, health checks, and OpenTelemetry.
- **AppHost**: Aspire orchestration for local development, with the API resource, readiness monitoring, and dashboard.
- **ServiceDefaults**: shared OpenTelemetry configuration, service discovery, and resilient HTTP clients. The API retains its existing health endpoints and application diagnostics. Automatic HTTP retries exclude unsafe methods such as POST.
- **Tests**: domain tests and HTTP integration tests against a real temporary SQLite database.

[Mediator](https://github.com/martinothamar/Mediator) 3 replaces MediatR. It is [MIT licensed](https://github.com/martinothamar/Mediator/blob/main/LICENSE) and generates dispatch/registration at build time. The generator lives only in the API project. Scoped mediator/handler lifetimes match EF Core. AutoMapper is replaced by explicit construction and response mapping.

Add a request and handler in Application, then expose it through an endpoint. The generator discovers handlers from the Application assembly. The pipeline records timing, enforces authorization requirements, then awaits validators before dispatch.

The previous generic command/query repository hierarchy is replaced by `IUserRepository`. A request scope shares one DbContext, cancellation propagates to database operations, and unique email constraints handle concurrent registration conflicts.

## Authentication and permissions

Passwords use ASP.NET Core Identity's versioned, salted PBKDF2 format with 210,000 iterations. Password hashes never appear in user response contracts. Application logs contain request names, outcomes, durations, and account IDs, without request bodies, passwords, or tokens.

Five failed logins lock an account for 15 minutes. Successful login resets the counter and records its timestamp. Optimistic concurrency prevents simultaneous account updates from overwriting login attempts; conflicting updates return 409 and can be retried. Token validation checks signature, algorithm, issuer, audience, expiry, and current account availability. Locked or password-expired accounts cannot use existing tokens.

Public registration always creates an ordinary account. No administrator is seeded. Role and permission assignment belong in a trusted provisioning workflow; the public API accepts no privilege fields. Read permissions are checked in the database, including inherited role permissions, so revocation takes effect without waiting for token expiry.

Registration and login share a per-IP fixed-window limit of 10 requests per minute. Set `RateLimiting:AuthenticationPermitLimit` to adjust it. The built-in limiter is local to one process; use an ingress/shared limiter when running multiple replicas. If hosting behind a proxy, configure trusted forwarded headers so the application can identify client IPs accurately.

## Databases and migrations

SQLite is the default. For SQL Server set both:

```text
Database__Provider=SqlServer
ConnectionStrings__Database=<your SQL Server connection string>
```

Provider-specific contexts and initial migrations are included for SQLite and SQL Server. Restore the pinned EF tool:

```sh
dotnet tool restore
```

Apply migrations explicitly outside Development, using the context that matches the configured provider:

```sh
dotnet ef database update --context SqlServerDbContext \
  --project src/BackendTemplate.Infrastructure --startup-project src/BackendTemplate.Api
```

The design-time factories read `ConnectionStrings__Database` from the environment. For SQLite, replace the context above with `SqliteDbContext` and provide an absolute database path when the working directory differs from the API project.

After model changes, add a migration for **each** context into `Persistence/Migrations/Sqlite` and `Persistence/Migrations/SqlServer`. To generate a SQL Server deployment script:

```sh
dotnet ef migrations script --idempotent --context SqlServerDbContext \
  --project src/BackendTemplate.Infrastructure --startup-project src/BackendTemplate.Api
```

Domain events run in-process after a successful database commit. The supplied registration handler logs the account ID. Add an outbox before using these events for durable email or message delivery.

The new initial migrations target fresh databases. This changes the old entity/password schema; existing data needs a deliberate migration. Legacy password hashes are not accepted by the new Identity hasher.

## Production configuration

Configure `Jwt__Key` through your deployment's secret store, plus `Jwt__Issuer`, `Jwt__Audience`, `AllowedHosts`, and `ConnectionStrings__Database`. A missing/short signing key or invalid JWT settings fail startup outside Development. `Jwt__LifetimeMinutes` defaults to 60 and accepts 1–120.

Production enables HSTS and HTTPS redirection. Configure certificates or a trusted TLS-terminating reverse proxy and its forwarded headers. Apply migrations as a deployment step before starting the application. SQLite deployments need a persistent writable database path; SQL Server suits deployments with multiple replicas.

```sh
docker build -t backend-template .
```

The container runs as the .NET image's non-root app user and listens on port 8080. Its default SQLite path is `/app/data/backend.db`; mount persistent writable storage at `/app/data`, or override the connection string for SQL Server. Pass production configuration at runtime; secrets are not baked into the image.

The AppHost configures the API's OTLP endpoint and authentication automatically. For standalone deployments, set `OTEL_EXPORTER_OTLP_ENDPOINT` to export logs, traces, and metrics to an OpenTelemetry collector. Without it, instrumentation remains available to listeners without console exporter noise.

This starter covers registration, login, and account reads. Email confirmation, password recovery, refresh tokens, account administration, and durable external event delivery are application-specific additions.

## Verify

```sh
dotnet build --configuration Release
dotnet test --configuration Release
dotnet format --verify-no-changes
```

HTTP tests exercise registration, validation, normalized duplicate detection, authentication, lockouts, token validation, ownership, direct/role permissions, revocation, response/log redaction, rate limiting, health checks, and OpenAPI. They apply real SQLite migrations rather than using EF's in-memory provider. SQL Server migration generation can be checked without a running database; running against SQL Server also needs that service.

GitHub Actions builds, checks formatting, runs tests, and publishes the API. Package versions are centralized in `Directory.Packages.props`; Dependabot groups related upgrades.
