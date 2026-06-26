# MultiTenantShop

> Multi-tenant e-commerce platform — SaaS for shop owners, built with .NET 10 + Blazor.

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
