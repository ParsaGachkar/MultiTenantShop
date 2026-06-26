# MultiTenantShop

> Multi-tenant e-commerce platform — SaaS for shop owners, built with .NET 10 + Blazor + Aspire.

![CI](https://github.com/ParsaGachkar/MultiTenantShop/actions/workflows/ci.yml/badge.svg)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)
[![Blazor](https://img.shields.io/badge/Blazor-SSR-512BD4)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![MongoDB](https://img.shields.io/badge/DB-MongoDB-green)](https://www.mongodb.com/)
![tests](https://img.shields.io/badge/tests-unit%20only-informational)

## Progress

- [x] Project scaffold (Clean Architecture + Aspire orchestration)
- [x] UI design doc (`docs/design.md`)
- [x] Domain model doc (`docs/domain/model.md`)
- [x] **Domain entities** — Product, Order, Customer, Cart, Payment, Shipment, Refund, Coupon, etc.
- [x] **Value objects** — Money, Address
- [x] **Enums** — OrderStatus, PaymentStatus, ShipmentStatus, RefundStatus, etc.
- [x] **ULID** primary keys
- [x] **CI** — GitHub Actions (build + test + coverage + test report)
- [ ] Tenant resolution middleware
- [ ] **MongoDB persistence** (replaces LiteDB)
- [ ] CQRS handlers (Application layer)
- [ ] API controllers
- [ ] Admin panel (Blazor SSR)
- [ ] Storefront layout (tenant-themed)
- [ ] Auth (ASP.NET Identity + Cookies)
- [ ] ZarinPal payment integration
- [ ] Email/SMS notifications
- [ ] Deployment to production

## Quick Start

```bash
dotnet build
dotnet test
dotnet run --project src/AppHost/MultiTenantShop.AppHost.csproj
```

## Monitoring UIs (via Aspire Dashboard)

| Resource | UI | Purpose |
|----------|-----|---------|
| MongoDB | Mongo Express (port 8081) | Browse collections, run queries, manage indexes |
| Redis | Redis Commander (port 8082) | View/edit keys, monitor memory, inspect streams |
| RabbitMQ | Management Plugin (port 15672) | Monitor queues, exchanges, message rates, DLQ |

All UIs are auto-provisioned by Aspire and accessible from the dashboard.

## Testing

### Unit Tests (Current)
- **Framework**: xUnit + Moq
- **Scope**: Repository logic, tenant store dual-mode logic, domain validation
- **Run**: `dotnet test`

### Integration Tests (Planned)
- **Framework**: xUnit + Aspire AppHost + Testcontainers
- **Scope**: End-to-end tenant resolution, MongoDB/Redis/RabbitMQ integration, API contracts
- **Run**: `dotnet test --filter Category=Integration` (requires Docker)
- **Status**: [ ] Not implemented — see [DECISIONS.md](DECISIONS.md#integration-tests)

See `AGENTS.md` for full developer guide and conventions.

## Decisions Needed (TODO)

- [ ] **Integration test strategy**: Aspire AppHost fixture (primary) + Testcontainers (fallback)
- [ ] **Code coverage target**: Line % / Branch % / Threshold for CI gate
- [ ] **MongoDB driver version**: v2.x (stable) vs v3.x (preview, DI-friendly)
- [ ] **Tenant provisioning API**: Admin-only vs self-service (future)
- [ ] **Event bus**: MediatR in-process vs RabbitMQ outbox pattern

## Research Needed

- [ ] **Coverage thresholds**: Industry standards for .NET SaaS (line vs branch, exclusions)
- [ ] **MongoDB v3 driver**: Breaking changes, DI integration, performance vs v2
- [ ] **Aspire integration testing**: Best practices for AppHost-based test fixtures
- [ ] **ULID vs UUIDv7**: Sortable ID comparison for distributed systems
- [ ] **Blazor SSR + WASM interop**: Optimal render modes for admin vs storefront