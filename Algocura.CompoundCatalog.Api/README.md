# Algocura Compound Catalog API

The **Algocura Compound Catalog API** is a RESTful microservice that manages chemical compounds used throughout the Algocura Drug Discovery Platform.

It provides CRUD operations, search and filtering capabilities, publishes integration events, and serves as the authoritative source for compound metadata used by downstream services.

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

# Project Structure

```
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
└── Dockerfile
```

---

# Compound Lifecycle

The catalog supports the following lifecycle states:

| Status | Description |
|---------|-------------|
| Draft | Initial registration |
| Registered | Available in catalog |
| UnderEvaluation | Being evaluated |
| Selected | Selected for downstream workflow |
| Rejected | Evaluation failed |
| Archived | Historical record |

---

# REST API

Base URL

```
/api/v1/compounds
```

## Search compounds

```
GET /api/v1/compounds
```

Optional query parameters

| Parameter | Description |
|-----------|-------------|
| search | Search by name, InChIKey, molecular formula or SMILES |
| status | Compound lifecycle status |
| skip | Records to skip |
| take | Maximum records |

Example

```
GET /api/v1/compounds?search=aspirin&status=Registered
```

---

## Get compound

```
GET /api/v1/compounds/{id}
```

---

## Register compound

```
POST /api/v1/compounds
```

---

## Update compound

```
PUT /api/v1/compounds/{id}
```

---

## Delete compound

```
DELETE /api/v1/compounds/{id}
```

---

# Integration Events

The service publishes the following events:

- CompoundRegistered
- CompoundUpdated
- CompoundStatusChanged
- CompoundDeleted

These events are published through the configured `IEventPublisher`.

---

# Database

The service uses SQL Server with Entity Framework Core migrations.

Database initialization performs:

1. Apply pending migrations
2. Seed reference compounds
3. Start accepting requests

Seeding is handled by:

```
CompoundCatalogSeeder
```

Database startup is managed by:

```
DatabaseInitializer
```

---

# Running Locally

Restore packages

```bash
dotnet restore
```

Build

```bash
dotnet build
```

Run

```bash
dotnet run
```

---

# Running with Docker

Build

```bash
docker compose build
```

Run

```bash
docker compose up -d
```

Stop

```bash
docker compose down
```

---

# OpenAPI

Swagger

```
http://localhost:7001/swagger
```

OpenAPI document

```
http://localhost:7001/openapi/v1.json
```

Health endpoint

```
http://localhost:7001/health
```

---

# Testing

Run all unit tests

```bash
dotnet test
```

Current test coverage includes:

- DbContext
- Database initialization
- Seed data
- Controller endpoints

---

# Logging

The service uses the built-in ASP.NET Core logging abstractions and emits structured logs for:

- Compound registration
- Updates
- Deletions
- Database initialization
- Migration execution
- Event publication

---

# Security

The API currently targets development scenarios.

Future enhancements include:

- Microsoft Entra ID authentication
- Role-based authorization
- API versioning
- Rate limiting
- Audit logging

---

# Future Enhancements

- Advanced chemical structure search
- Similarity search
- Molecular fingerprint indexing
- Bulk import/export
- Versioned compounds
- Soft delete
- Optimistic concurrency
- Event replay
- GraphQL endpoint
- OData support

---

# License

Copyright © Algocura Biotech LLC.

All rights reserved.