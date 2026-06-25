# Multi-Tenancy Strategy

## Options Comparison

```mermaid
quadrantChart
  title Tenant Isolation Strategies
  x-axis Low Isolation --> High Isolation
  y-axis Low Cost --> High Cost
  quadrant-1 "Sweet Spot"
  quadrant-2 "Best Isolation"
  quadrant-3 "Start Here"
  quadrant-4 "Avoid"
  Shared-DB: [0.2, 0.3]
  Schema-per-Tenant: [0.5, 0.5]
  DB-per-Tenant: [0.9, 0.9]
  Hybrid: [0.6, 0.6]
```

## Decision

**Default: Shared DB with TenantId** — opt-in per-tenant databases via connection string stored in the shared DB.

## Dual-Mode Strategy

```mermaid
flowchart TD
    A[Request arrives] --> B[Resolve Tenant]
    B --> C{"Tenant has<br/>custom ConnectionString?"}
    C -->|No| D[Use shared DB]
    C -->|Yes| E[Use per-tenant DB]

    D --> F[Filter by TenantId]
    E --> G[No filter needed - physical isolation]

    F --> H[Return tenant-scoped data]
    G --> H
```

**How it works:**

| Mode | Trigger | Behavior |
|---|---|---|
| **Shared DB** (default) | Tenant record has null `ConnectionString` | Single database, every document has `TenantId`, queries filter by it |
| **DB-per-Tenant** (opt-in) | Tenant record has a `ConnectionString` value | Separate database for that tenant, no tenant filter needed |

## Per-Tenant Connection Strings in the Shared DB

Only two environment variables exist:

```bash
DatabaseProvider=LiteDB                          # or MongoDB
ConnectionStrings__Default=Filename=Data/MultiTenantShop.db
```

The shared DB contains a `Tenants` collection. Each tenant record stores its own optional connection string:

```json
{
  "slug": "acme-corp",
  "tenantId": "a1b2c3d4-...",
  "name": "Acme Corporation",
  "connectionString": null
},
{
  "slug": "mega-store",
  "tenantId": "e5f6g7h8-...",
  "name": "Mega Store Inc.",
  "connectionString": "mongodb://megastore-db:27017/megastore"
}
```

On startup the app reads the `Tenants` collection from the shared DB. Tenants with a `connectionString` get their own database. Tenants with null share the default DB.

**Adding or moving a tenant** = insert/update a document in the `Tenants` collection. No restart required.

## Tenant Resolution Flow

### Component Overview

```mermaid
classDiagram
    class TenantRecord {
        +string Slug
        +Guid TenantId
        +string Name
        +string? ConnectionString
    }

    class ITenantContext {
        <<interface>>
        +Guid TenantId
        +string? ConnectionString
    }

    class TenantMiddleware {
        +InvokeAsync(HttpContext context)
    }

    class TenantStore {
        +GetCollection(tenantId)
    }

    class TenantResolver {
        +ResolveAsync(HttpContext context)
        +GetConnectionString(slug)
    }

    TenantMiddleware --> TenantResolver : resolves
    TenantResolver --> ITenantContext : sets
    TenantStore --> ITenantContext : reads for DB selection

    note for TenantResolver "Reads TenantRecord from shared DB\nChecks ConnectionString field"
    note for TenantStore "If ConnectionString is set:\n  per-tenant DB, no filter\nElse:\n  shared DB + TenantId filter"
```

### Connection Resolution Logic

```mermaid
flowchart LR
    A[TenantResolver] --> B[Read TenantRecord\nfrom shared DB]
    B --> C{"Tenant has\nConnectionString?"}
    C -->|No| D[Use shared DB\n+ TenantId filter]
    C -->|Yes| E[Open per-tenant DB]

    D --> F[Query returns tenant data]
    E --> F
```

### Sequence

```mermaid
sequenceDiagram
    participant C as Client
    participant MW as Tenant Middleware
    participant TR as TenantResolver
    participant SDB as Shared DB
    participant TDB as Tenant DB

    C->>MW: GET /products (Host: acme.myshop.com)
    MW->>TR: Resolve tenant from host
    TR->>SDB: Read TenantRecord for slug
    SDB-->>TR: TenantRecord (connectionString = null)
    TR-->>MW: Use shared DB + TenantId
    MW->>SDB: Query WHERE TenantId = @id
    SDB-->>C: Tenant-scoped results

    Note over MW,TDB: Tenant with dedicated DB:
    C->>MW: GET /orders (Host: mega.myshop.com)
    MW->>TR: Resolve tenant
    TR->>SDB: Read TenantRecord
    SDB-->>TR: TenantRecord (connectionString = "mongodb://...")
    TR-->>MW: Use per-tenant DB
    MW->>TDB: Query (no TenantId filter needed)
    TDB-->>C: Results
```

```plantuml
@startuml
skinparam componentStyle rectangle

package "Tenant Resolution" {
  [TenantMiddleware] --> [TenantResolver]
  [TenantResolver] --> [ITenantContext]
  note left of [TenantResolver]
    Strategies:
    - Host header (shop1.myshop.com)
    - X-Tenant-Id header
    - Path prefix (/shop1/api/...)

    Reads TenantRecord from shared DB
    Checks ConnectionString field
  end note
}

package "Data Access" {
  [TenantStore] --> [ITenantContext]
  [Repository] --> [TenantStore]
  note right of [TenantStore]
    If ConnectionString is set -->
      separate DB, no filter
    Else --> shared DB + TenantId filter
  end note
}

package "Domain" {
  interface "ITenantScoped" {
    + Guid TenantId
  }
  class "Product" {
    + Guid TenantId
  }
  class "Order" {
    + Guid TenantId
  }
}

ITenantScoped <|.. Product
ITenantScoped <|.. Order
@enduml
```

## Tenant Isolation per Database

| Strategy | LiteDB | MongoDB |
|---|---|---|
| **Shared DB** (default) | Single `.db` file, documents include `TenantId` field, queries filter by it | Single database, `TenantId` field on every document |
| **Per-tenant** (opt-in) | Separate `.db` file per tenant | Separate database per tenant |

## Resolved Questions

- **Self-service provision?** No — tenants are provisioned by platform admins only.
- **Tenant-specific themes/branding?** Yes — each tenant can customize their storefront theme and branding. Approach: Tailwind CSS variables scoped via `TenantShopLayout`, admin panel unchanged. See [tech-stack.md](tech-stack.md#tenant-theming-approach).
- **Cross-tenant reporting?** Not for tenants. Cross-tenant admin views are available to platform admins only.
- **Tenant onboarding flow:** `Signup → Choose plan → Pay → Setup DNS → Ready` — DNS maps to the tenant's shop front (e.g., `acme.myshop.com`). The admin panel is on our domain (e.g., `admin.multitenantshop.com`) and shared by all tenants.
