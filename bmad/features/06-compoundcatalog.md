# FR-06: Algocura.CompoundCatalog.Api — BMAD feature brief

**Status:** Draft, source-derived. **Technical evidence:** [`specs/Algocura.CompoundCatalog.Api/spec.md`](../../specs/Algocura.CompoundCatalog.Api/spec.md)

## Documented purpose and capabilities

# Algocura Compound Catalog API

The **Algocura Compound Catalog API** is a cloud-native RESTful microservice that serves as the authoritative source for chemical compound metadata within the Algocura Drug Discovery Platform.

It provides CRUD operations, search and filtering capabilities, publishes integration events through Azure Service Bus, and enables downstream microservices to consume consistent compound information.

For an overview of the complete platform architecture, see the [Solution README](../README.md).

---

# Architecture

The Compound Catalog API manages the lifecycle of chemical compounds. It persists compound data in SQL Server using Entity Framework Core and publishes integration events through Azure Service Bus to notify downstream services of changes.

```text
                   Client Applications
                           │
                           ▼
                  ASP.NET Core API
                           │
                    CompoundsController
                           │
        ┌──────────────────┴──────────────────┐
        │                                     │
        ▼                                     ▼
 Entity Framework Core                 Integration Events
 (CompoundCatalogDbContext)             (IEventPublisher)
        │                                     │
        ▼                                     ▼
    SQL Server                      Azure Service Bus
                                              │
                                              ▼
                    Screening API
                    AI Discovery API
                    Workflow API
                    Notification API
```

---

# Features

- RESTful ASP.NET Core API
- OpenAPI / Swagger documentation
- Entity Framework Core with SQL Server
- Automatic database migrations
- Seed data initialization
- Compound search
- Status filtering
- Pagination
- Health checks
- Docker support
- Azure Service Bus integration
- Integration event publishing
- Structured logging

---

# Technology Stack

| Technology | Version |
|------------|----------|
| .NET | 10 |
| ASP.NET Core | 10 |
| Entity Framework Core | 10 |
| SQL Server | 2022 |
| Azure Service Bus | Emulator / Azure |
| Docker | Latest |
| OpenAPI | Microsoft.AspNetCore.OpenApi |

---

# Dependencies

This service depends on the following infrastructure components.

| Component | Purpose |
|-----------|---------|
| SQL Server | Persistent storage for compound data |
| Azure Service Bus | Integration event publishing |
| Docker | Local containerized development |
| OpenAPI | Interactive API documentation |

---

# Project Structure

```text
Algocura.CompoundCatalog.Api
│
├── Contracts
├── Controllers
├── Data
│   ├── CompoundCatalogDbContext
│   ├── CompoundCatalogSeeder
│   └── DatabaseInitializer
│
├── Domain
├── Events
├── Mapping
├── Messaging
├── OpenApi
├── Program.cs
├── Dockerfile
└── README.md
```

---

# Configuration

The service uses the standard ASP.NET Core configuration system.

Configuration is loaded from:

```text
appsettings.json
appsettings.Development.json
Environment Variables
```

Common configuration includes:

- SQL Server connection string
- Azure Service Bus configuration
- Logging
- OpenAPI
- Health Checks

Sensitive configuration should be provided through environment variables, Azure Key Vault, or .NET User Secrets during local development.

---

# Database

The service uses SQL Server with Entity Framework Core migrations.

During application startup the database is automatically initialized by:

```text
DatabaseInitializer
```

Initialization performs the following steps:

1. Apply pending Entity Framework Core migrations.
2. Seed reference compound data.
3. Start the application.

Reference data is seeded by:

```text
CompoundCatalogSeeder
```

---

# Compound Lifecycle

The catalog supports the following lifecycle states.

| Status | Description |
|---------|-------------|
| Draft | Initial registration |
| Registered | Available in the catalog |
| UnderEvaluation | Being evaluated |
| Selected | Selected for downstream workflows |
| Rejected | Evaluation failed |
| Archived | Historical record |

---

# API Overview

Base URL

```text
/api/v1/compounds
```

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/compounds` | Search compounds with filtering and pagination. |
| GET | `/api/v1/compounds/{id}` | Retrieve a compound by identifier. |
| POST | `/api/v1/compounds` | Register a new compound. |
| PUT | `/api/v1/compounds/{id}` | Update an existing compound. |
| DELETE | `/api/v1/compounds/{id}` | Delete a compound. |

## Observed HTTP endpoints (where explicitly present in technical spec)
See the linked technical specification and route index; no endpoints extracted from this section.

## Required behavior for regeneration
- Preserve documented operations, domain entities, input/output contracts, validation, status codes, authorization, persistence, and integration behavior **after verifying their implementation**.
- Reconstruct user-facing journeys and test cases from actual UI and service behavior, not from endpoint names alone.
- Keep deterministic prototype logic distinct from scientifically validated conclusions.

## Acceptance criteria (to complete before coding)
- [ ] Each documented operation has concrete valid/invalid request and response examples.
- [ ] Business rules and state transitions have approved decision tables.
- [ ] Authorization matrix, audit events, and integration failure handling are specified.
- [ ] Relevant tests are mapped to business requirements and parity baselines.
- [ ] Product/scientific owner signs off on unknown behavior.

## Open questions
What are the exact rules, failure modes, domain definitions, security constraints, and frontend journeys? These cannot be assumed from the inventory alone.
