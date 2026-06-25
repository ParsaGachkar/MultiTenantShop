# MultiTenantShop — Agent Instructions

## Project Overview
Multi-tenant e-commerce platform (SaaS for shop owners). Each tenant manages their own catalog, orders, payments. Customers browse individual shops or marketplace.

**Architecture**: Clean Architecture + Vertical Slices + Modular Monolith  
**Runtime**: .NET 10 / ASP.NET Core  
**Frontend**: Blazor Web App (static SSR default, Auto for authenticated pages)  
**Orchestration**: .NET Aspire

## Solution Structure
```
src/
├── Core/                    # Domain models, shared kernel (empty — add entities, VOs, domain events, repo interfaces here)
├── Application/             # CQRS handlers, DTOs, validation (empty — depends on Core + Infrastructure)
├── Infrastructure/          # LiteDB/MongoDB, tenant providers, payments, email/SMS, cache (empty — depends on Core)
├── Web/
│   ├── MultiTenantShop.Web/         # Main app: API + tenant resolution middleware + Blazor SSR
│   └── MultiTenantShop.Web.Client/  # Blazor WASM client (minimal — just bootstraps)
├── AppHost/                 # Aspire orchestration (run this for full local stack)
└── ServiceDefaults/         # Shared DI: OpenTelemetry, health checks, HTTP resilience
tests/
└── MultiTenantShop.Tests/   # xUnit (empty — add integration/unit tests here)
```

## Key Architectural Decisions (from `docs/`)
- **Multi-tenancy**: Shared DB with `TenantId` field (default). Opt-in per-tenant DB via `ConnectionString` stored in shared DB's `Tenants` collection. See `docs/multi-tenancy.md`.
- **Database**: LiteDB (local dev) / MongoDB (production) — switchable via `DatabaseProvider` env var.
- **Tenant Resolution**: Host header (`shop1.myshop.com`), `X-Tenant-Id` header, or path prefix (`/shop1/api/...`). Middleware sets `ITenantContext.TenantId`.
- **Theming**: Per-tenant Tailwind CSS variables via `TenantShopLayout.razor`; admin panel uses fixed theme.
- **Payments**: ZarinPal. Notifications: Private SMTP (email), Kavenegar (SMS).
- **Auth**: ASP.NET Core Identity + Cookies.

## Developer Commands
```bash
# Build entire solution
dotnet build

# Run with Aspire (recommended — spins up Redis, RabbitMQ, etc.)
dotnet run --project src/AppHost/MultiTenantShop.AppHost.csproj

# Run web app directly (no Aspire dependencies)
dotnet run --project src/Web/MultiTenantShop.Web/MultiTenantShop.Web.csproj

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Environment Variables
| Variable | Purpose | Default |
|---|---|---|
| `DatabaseProvider` | `LiteDB` or `MongoDB` | `LiteDB` |
| `ConnectionStrings__Default` | Shared DB connection | `Filename=Data/MultiTenantShop.db` |
| `OTEL_EXPORTER_OTLP_ENABLED` | Enable OTLP export | `false` |

## Adding Code — Where Things Go
| Concern | Project |
|---|---|
| Domain entities, value objects, domain events, repository interfaces | `Core` |
| Commands, queries, validators, DTOs, MediatR handlers | `Application` |
| DB implementations, tenant provider, payment gateway, email/SMS, cache | `Infrastructure` |
| API controllers, tenant middleware, exception handling, Blazor components | `Web/MultiTenantShop.Web` |
| Blazor WASM pages/components (rare — mostly SSR) | `Web/MultiTenantShop.Web.Client` |

## Testing Notes
- Tests reference `Application` and `Web` projects.
- Use Aspire for integration tests (see `src/AppHost`).
- No tests exist yet — establish patterns when adding first tests.

## Conventions
- **Nullable**: Enabled globally (`<Nullable>enable</Nullable>`)
- **ImplicitUsings**: Enabled globally
- **Target Framework**: `net10.0` everywhere
- **Package References**: Centralized in `Directory.Packages.props` (not yet created — use csproj for now)
- **No `Program.cs` in Core/Application/Infrastructure** — they're class libraries
- **Primary Keys**: Always ULID (via `Ulid.NewUlid().ToString()`). Never `Guid`, never `int`. The `Ulid` NuGet package is referenced in Core. Use `Ulid.NewUlid().ToString()` for all entity ID generation.

## Common Pitfalls
- **Empty projects**: Core, Application, Infrastructure have only csproj files. Implementation goes there.
- **Tenant context**: Always resolve via `ITenantContext` (set by middleware), never from raw headers in handlers.
- **DB per tenant**: Check `TenantRecord.ConnectionString` — if set, use that DB without `TenantId` filter.
- **Aspire required** for Redis/RabbitMQ/observability in local dev.

## References
- `docs/architecture/overview.md` — C4 diagrams, component diagram
- `docs/multi-tenancy.md` — Tenant resolution flow, dual-mode strategy
- `docs/tech-stack.md` — Full stack table, theming approach
- `docs/decisions/ADR-001-multi-tenancy-strategy.md` — ADR for tenancy

## Commit Message Convention
Follow Conventional Commits: `<type>(<scope>): <subject>`
- **Types**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `build`, `ci`
- **Scope**: project/component affected (e.g., `web`, `core`, `appHost`, `infra`)
- **Subject**: Imperative mood, lowercase, no period, ≤50 chars
- **Body**: Explain *what* and *why* (not *how*), wrap at 72 chars
- **Match scope to changes**: Message must accurately reflect all modified files

Examples:
- `feat(web): add tenant resolution middleware`
- `fix(appHost): enable OTLP exporter for Aspire dashboard`
- `docs(architecture): add C4 component diagram`