# MultiTenantShop

> Multi-tenant e-commerce platform — SaaS for shop owners, built with .NET 10 + Blazor.

![CI](https://github.com/anomalyco/MultiTenantShop/actions/workflows/ci.yml/badge.svg)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)
[![Blazor](https://img.shields.io/badge/Blazor-SSR-512BD4)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![LiteDB](https://img.shields.io/badge/DB-LiteDB%20%2F%20MongoDB-green)](https://www.litedb.org/)
![tests](https://img.shields.io/badge/tests-92%20passing-brightgreen)

## Progress

- [x] Project scaffold (Clean Architecture + Aspire orchestration)
- [x] UI design doc (`docs/design.md`)
- [x] Domain model doc (`docs/domain/model.md`)
- [x] **Domain entities** — Product, Order, Customer, Cart, Payment, Shipment, Refund, Coupon, etc.
- [x] **Value objects** — Money, Address
- [x] **Enums** — OrderStatus, PaymentStatus, ShipmentStatus, RefundStatus, etc.
- [x] **92 unit tests** — all passing
- [x] **ULID** primary keys
- [x] **CI** — GitHub Actions (build + test + coverage + test report)
- [ ] Tenant resolution middleware
- [ ] LiteDB persistence
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

See `AGENTS.md` for full developer guide and conventions.
