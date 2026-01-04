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

## Query Parameters

### GET /api/v1/compounds

| Parameter | Type | Description |
|-----------|------|-------------|
| search | string | Search by Name, Canonical SMILES, InChIKey, or Molecular Formula. |
| status | CompoundStatus | Filter by lifecycle status. |
| skip | int | Number of records to skip. |
| take | int | Maximum number of records returned (default: 50). |

---

# OpenAPI

Interactive API documentation is available when the service is running locally.

| Resource | URL |
|----------|-----|
| Swagger UI | http://localhost:7001/swagger |
| OpenAPI Specification | http://localhost:7001/openapi/v1.json |
| Health Endpoint | http://localhost:7001/health |

The generated OpenAPI specification is the authoritative source for request and response schemas.

---

# Integration Events

The service publishes the following integration events through the configured `IEventPublisher`.

| Event | Description |
|------|-------------|
| CompoundRegistered | Published when a compound is created. |
| CompoundUpdated | Published when compound metadata is updated. |
| CompoundStatusChanged | Published when the lifecycle status changes. |
| CompoundDeleted | Published when a compound is removed. |

These events enable loose coupling between the Compound Catalog API and downstream microservices.

---

# Running Locally

Restore packages.

```bash
dotnet restore
```

Build the project.

```bash
dotnet build
```

Run the application.

```bash
dotnet run
```

---

# Running with Docker

Build the container.

```bash
docker compose build
```

Start the application and supporting infrastructure.

```bash
docker compose up -d
```

Stop the environment.

```bash
docker compose down
```

---

# Testing

Run all unit tests.

```bash
dotnet test
```

Current test coverage includes:

- Entity Framework Core DbContext
- Database initialization
- Seed data
- Controller endpoints

---

# Logging

The service uses `Microsoft.Extensions.Logging` for structured logging.

Logging includes:

- Application startup
- Database initialization
- Entity Framework Core migrations
- Compound creation
- Compound updates
- Compound deletion
- Integration event publication
- Unhandled exceptions

---

# Security

The current implementation is intended for local development.

Future enhancements include:

- Microsoft Entra ID authentication
- JWT Bearer authentication
- Role-based authorization
- API versioning
- Rate limiting
- Audit logging

---

# Future Enhancements

- Chemical structure similarity search
- Molecular fingerprint indexing
- Bulk import/export
- Versioned compounds
- Soft delete
- Optimistic concurrency
- Event replay
- GraphQL endpoint
- OData support
- Distributed caching
- Metrics and distributed tracing

---

# Related Documentation

| Document | Description |
|----------|-------------|
| [Solution README](../README.md) | Overall platform architecture and microservices |
| OpenAPI Specification | http://localhost:7001/openapi/v1.json |

---

# License

Copyright © Algocura Biotech LLC.

All rights reserved.