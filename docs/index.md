# MultiTenantShop — Documentation

## Brainstorming Sessions

- [Session 1 — Project Kickoff (2026-06-25)](/README.md#session-1--project-kickoff-2026-06-25)

## Architecture

| Document | Diagrams |
|---|---|
| [Architecture Overview](architecture/overview.md) | C4 context, container, sequence diagrams |
| [Multi-Tenancy Strategy](multi-tenancy.md) | Strategy decision tree, EF Core approach |
| [Tech Stack](tech-stack.md) | Technology decisions, library mindmap |

## Domain

| Document | Diagrams |
|---|---|
| [Domain Model](domain/model.md) | ERD, class diagrams, state machines, workflows |

## Decisions

| ADR | Status |
|---|---|
| [ADR-001: Multi-Tenancy Strategy](decisions/ADR-001-multi-tenancy-strategy.md) | Accepted ✅ |

---

To regenerate PlantUML diagrams locally: `plantuml docs/**/*.md`
