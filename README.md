# PayCore

ASP.NET Core 8 Web API for payment processing. Changed for testing.

## Solution structure

```
PayCore/
├── src/
│   ├── PayCore.API/           # Web API host (controllers, middleware, config)
│   ├── PayCore.Core/          # Domain entities and repository interfaces
│   └── PayCore.Infrastructure/ # EF Core, Npgsql, repository implementations
└── PayCore.sln
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- PostgreSQL 15+

## Getting started

### 1. Configure the database

Copy `appsettings.json` and set your connection string and JWT secret:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=paycore;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Key": "your-32-char-minimum-secret-here",
    "Issuer": "PayCore",
    "Audience": "PayCoreClients"
  }
}
```

> **Never** commit real secrets. Use `dotnet user-secrets` in development.

### 2. Apply EF Core migrations

```bash
cd src/PayCore.API
dotnet ef database update --project ../PayCore.Infrastructure
```

### 3. Run the API

```bash
dotnet run --project src/PayCore.API
```

Swagger UI is available at `http://localhost:5000/swagger` in Development.

Health check: `GET /health`

## Architecture

| Layer | Responsibility |
|-------|---------------|
| `PayCore.Core` | Domain entities (`BaseEntity`), repository interfaces (`IRepository<T>`) |
| `PayCore.Infrastructure` | EF Core `AppDbContext`, generic `Repository<T>`, migrations |
| `PayCore.API` | Controllers, `GlobalExceptionHandler` (RFC 7807), Serilog, Swagger + JWT, CORS |

## Adding a new resource

1. Add entity in `PayCore.Core/Entities/`
2. Add interface in `PayCore.Core/Interfaces/` if you need custom queries
3. Register `DbSet<T>` in `AppDbContext`
4. Create a concrete repository in `PayCore.Infrastructure/Repositories/` (or rely on generic `Repository<T>`)
5. Add controller in `PayCore.API/Controllers/`
6. Run `dotnet ef migrations add <Name> --project src/PayCore.Infrastructure --startup-project src/PayCore.API`

## Environment variables

| Variable | Default | Notes |
|----------|---------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Set to `Development` locally |
| `ConnectionStrings__DefaultConnection` | — | Override via env in prod |
| `Jwt__Key` | — | 32+ character secret |

## License

MIT
