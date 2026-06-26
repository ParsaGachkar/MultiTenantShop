# Brainstorming Sessions

## Session 1 — Project Kickoff (2026-06-25)

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
| Architecture style | Clean Architecture + Vertical Slices + Modular Monolith | ✅ Decided |

**Initial architecture proposal:**

```
MultiTenantShop/
├── Core/                    # Domain models, shared kernel
├── Infrastructure/          # LiteDB/MongoDB, tenant providers, payments
├── Application/             # CQRS handlers, DTOs, validation
├── Web/                     # API + tenant resolution middleware
└── Tests/
```

## Session 2 — Design Vibe (2026-06-26)

**UI personality:** Competent, calm, fast. Focus on Persian-first RTL with Vazirmatn font.

**Key decisions:**

| Question | Options | Status |
|---|---|---|
| UI framework | DaisyUI on Tailwind CSS | ✅ Decided |
| Font | Vazirmatn (covers Persian + Latin) | ✅ Decided |
| Icons | Heroicons (nav) + Lucide (inline) | ✅ Decided |
| RTL | DaisyUI dir attribute, user-preference toggle | ✅ Decided |
| Dark mode | DaisyUI data-theme toggle, per-user | ✅ Decided |

## Session 3 — Domain Models (2026-06-26)

**Added models:** Address, Coupon, InventoryMovement, Shipment, Refund, CustomerAddress. Removed ProductPrice (replaced with embedded Money value object).
