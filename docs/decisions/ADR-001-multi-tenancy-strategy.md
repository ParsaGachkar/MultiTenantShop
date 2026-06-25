# ADR-001: Multi-Tenancy Strategy

**Status:** Accepted ✅  
**Date:** 2026-06-25  
**Deciders:** [Team]

## Context

We need to choose a multi-tenancy isolation strategy for the e-commerce platform. The choice affects cost, scalability, development complexity, and compliance.

## Options Considered

### 1. Shared Database with TenantId Field

- Single DB, all tenants in same collections, every document has a `TenantId`
- Queries always filter by `TenantId`
- **Pros:** Cheapest, simplest operations, easy cross-tenant queries
- **Cons:** Weakest isolation, risk of data leaks, harder to scale

### 2. Collection-per-Tenant

- One database, each tenant gets their own collection namespace
- `tenant1.products`, `tenant2.products`
- **Pros:** Good logical isolation, can restore individual tenants
- **Cons:** Collection management overhead, connection pooling limits

### 3. Database-per-Tenant

- Each tenant gets their own database (separate `.db` file for LiteDB, separate DB for MongoDB)
- **Pros:** Strongest isolation, easy backup/restore per tenant, compliance friendly
- **Cons:** Highest cost, connection management, resource overhead

### 4. Hybrid

- Combine approaches: shared DB for catalog (read-heavy, low sensitivity), DB-per-tenant for orders/payments (high sensitivity)
- **Pros:** Best of both worlds, right-sized isolation
- **Cons:** Most complex, cross-collection queries span databases

## Decision

We will use **Shared Database with TenantId Field** as the default strategy.

A **Database-per-Tenant** mode is supported as an opt-in override. Per-tenant connection strings are stored in the shared DB in a `Tenants` collection, **not in environment variables** — this allows adding or moving tenants without restarting the application.

**Why shared DB as default:**
- Lowest operational cost and complexity to start
- Document DBs naturally support tenant isolation via `TenantId` field on every document
- Single database to backup, single connection string
- Easy cross-tenant admin queries with tenant filter

**Why per-tenant connection strings in the DB (not env vars):**
- Adding a tenant with a dedicated DB = insert a document, no restart
- Moving a tenant from shared to dedicated = update a document, no restart
- Scales to thousands of tenants without operational burden
- Configuration stays in the data plane, not the deployment plane

**Env vars used (only for bootstrap):**
- `DatabaseProvider` — switches between LiteDB (local dev) and MongoDB (production)
- `ConnectionStrings__Default` — bootstrap connection to the shared DB

### Implementation Approach

```
Env vars (bootstrap only):
  DatabaseProvider          → LiteDB or MongoDB
  ConnectionStrings__Default → shared DB connection

Startup:
  Connect to shared DB using ConnectionStrings__Default
  Read Tenants collection → cached tenant configs

Request flow:
  Resolve tenant slug from request
  Lookup TenantRecord in cache (TenantId, ConnectionString?)
  ConnectionString is set? → create isolated DB instance
  Otherwise → use shared DB + filter by TenantId
```

In the shared DB, every document includes a `TenantId` field — queries always filter by it. When a per-tenant connection string is set, a separate database instance is created for that tenant and no filter is needed (physical isolation).

## Consequences

| Approach | Isolation | Cost | Complexity | Scale |
|---|---|---|---|---|
| Shared DB (default) | Low | Low | Low | Medium |
| DB-per-Tenant (opt-in) | High | High | Medium | High |

## Open Questions

- What compliance requirements exist (PCI, GDPR, etc.)?
- Expected tenant count in year 1? Year 5?
- Will tenants have custom data schemas/extensions?
