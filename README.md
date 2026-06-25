# MultiTenantShop

> Multi-tenant e-commerce platform — SaaS for shop owners, built with .NET.

## 🧠 Brainstorming Sessions

This project is in active design phase. All architectural decisions, trade-offs, and open questions are documented here and in `docs/`.

### Session 1 — Project Kickoff (2026-06-25)

**What are we building?**
A multi-tenant e-commerce platform where each tenant is a **shop owner** who manages their own catalog, processes orders, and accepts payments. End customers browse individual shops (or a marketplace view) and purchase products.

**Key brainstorming questions:**

| Question | Options | Status |
|---|---|---|
| Tenant isolation level | Shared DB (default), DB-per-tenant opt-in via env var | ✅ Decided |
| Frontend | Blazor Web App | ✅ Decided |
| Database | LiteDB (local dev) / MongoDB (production) — switchable via `DatabaseProvider` env var | ✅ Decided |
| Deployment model | Single deployment + multi-tenant routing | ✅ Decided |
| Payment provider | ZarinPal | ✅ Decided |
| Architecture style | Clean Architecture / Vertical Slices / Modular Monolith | 🔴 Open |

**Initial architecture proposal:**

```
MultiTenantShop/
├── Core/                    # Domain models, shared kernel
├── Infrastructure/          # LiteDB/MongoDB, tenant providers, payments
├── Application/             # CQRS handlers, DTOs, validation
├── Web/                     # API + tenant resolution middleware
└── Tests/
```

See `docs/` for detailed architecture diagrams, domain models, and ADRs.
