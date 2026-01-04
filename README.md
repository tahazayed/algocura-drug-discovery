# Algocura Drug Discovery Platform

The **Algocura Drug Discovery Platform** is a cloud-native, event-driven microservices platform designed to accelerate computational drug discovery by integrating compound management, virtual screening, AI-assisted analysis, laboratory workflows, and scientific data management.

Each microservice owns its data and communicates asynchronously through Azure Service Bus while exposing REST APIs for synchronous operations.

---

# Documentation

This repository contains multiple independently deployable microservices. Each service maintains its own README containing implementation details, API documentation, configuration, and local development instructions.

| Service | Documentation |
|----------|---------------|
| Compound Catalog API | [Algocura.CompoundCatalog.Api](./Algocura.CompoundCatalog.Api/README.md) |
| Screening API | Coming Soon |
| AI Discovery API | Coming Soon |
| Assay Management API | Coming Soon |
| Workflow API | Coming Soon |
| Notification API | Coming Soon |
| Reporting API | Coming Soon |
| Search API | Coming Soon |
| Audit API | Coming Soon |
| Identity API | Coming Soon |

---

# Solution Architecture

```
                                      +----------------------+
                                      |      API Gateway     |
                                      | (Azure APIM / YARP)  |
                                      +----------+-----------+
                                                 |
        -----------------------------------------------------------------------
        |             |             |             |             |              |
        v             v             v             v             v              v

+----------------+ +----------------+ +----------------+ +----------------+ +----------------+
| Compound        | | Screening      | | AI Discovery   | | Assay          | | Identity       |
| Catalog API     | | API            | | API            | | Management API | | API            |
+--------+--------+ +--------+-------+ +--------+-------+ +--------+-------+ +--------+-------+
         |                   |                  |                  |                  |
         -------------------------------------------------------------------------------
                                         |
                                  Azure Service Bus
                                         |
        -------------------------------------------------------------------------------
        |              |                 |                 |                 |
        v              v                 v                 v                 v

+----------------+ +----------------+ +----------------+ +----------------+ +----------------+
| Workflow       | | Notification   | | Reporting      | | Search         | | Audit          |
| API            | | API            | | API            | | API            | | API            |
+----------------+ +----------------+ +----------------+ +----------------+ +----------------+

                                         |
                          -----------------------------------
                          |                                 |
                          v                                 v

                  SQL Server Databases               Azure Blob Storage
                          |
                          v
                  Entity Framework Core
```

---

# Microservices

| Service | Responsibility | Documentation |
|----------|----------------|---------------|
| Compound Catalog API | Master repository for chemical compounds and lifecycle management | [README](./Algocura.CompoundCatalog.Api/README.md) |
| Screening API | Virtual screening and candidate selection | Coming Soon |
| AI Discovery API | AI/ML prediction and prioritization | Coming Soon |
| Assay Management API | Biological assay management | Coming Soon |
| Workflow API | Drug discovery workflow orchestration | Coming Soon |
| Search API | Compound and experiment search | Coming Soon |
| Notification API | Email, Teams, and event notifications | Coming Soon |
| Reporting API | Analytics and reporting | Coming Soon |
| Audit API | Audit trail and compliance | Coming Soon |
| Identity API | Authentication and authorization | Coming Soon |

---

# Event-Driven Architecture

The platform uses Azure Service Bus for asynchronous communication between services.

Typical integration events include:

- CompoundRegistered
- CompoundUpdated
- CompoundDeleted
- CompoundStatusChanged
- ScreeningStarted
- ScreeningCompleted
- AssayCreated
- AssayCompleted
- PredictionCompleted
- WorkflowCompleted

Each microservice publishes domain events without directly depending on other services.

---

# Data Ownership

Each microservice owns its own database.

```
Compound Catalog API
    SQL Server

Screening API
    SQL Server

AI Discovery API
    SQL Server

Assay Management API
    SQL Server

Reporting API
    SQL Server
```

No databases are shared between services.

---

# Technology Stack

| Area | Technology |
|------|------------|
| Runtime | .NET 10 |
| API Framework | ASP.NET Core |
| Data Access | Entity Framework Core |
| Database | SQL Server |
| Messaging | Azure Service Bus |
| Storage | Azure Blob Storage |
| API Gateway | Azure API Management (planned) |
| Containers | Docker |
| Orchestration | Kubernetes (planned) |
| API Documentation | OpenAPI |
| Testing | xUnit, FluentAssertions, NSubstitute |
| Logging | Microsoft.Extensions.Logging |
| Health Monitoring | ASP.NET Core Health Checks |

---

# Solution Structure

```
Algocura.DrugDiscovery
│
├── README.md
├── compose.yaml
│
├── Algocura.CompoundCatalog.Api
│   └── README.md
│
├── Algocura.Screening.Api
├── Algocura.AIDiscovery.Api
├── Algocura.AssayManagement.Api
├── Algocura.Workflow.Api
├── Algocura.Notification.Api
├── Algocura.Reporting.Api
├── Algocura.Search.Api
├── Algocura.Audit.Api
├── Algocura.Identity.Api
│
└── tests
```

---

# Infrastructure

The local development environment uses Docker Compose to provision:

- SQL Server
- Azure Service Bus Emulator
- Azurite Storage Emulator
- Compound Catalog API
- Supporting infrastructure services

Additional microservices will be added to the compose environment as they are implemented.

---

# Design Principles

- Microservices architecture
- Domain-Driven Design (DDD)
- Event-driven communication
- Database-per-service
- RESTful APIs
- OpenAPI-first development
- Containerized deployment
- Cloud-native architecture
- Observability and structured logging
- Health monitoring
- Automated testing
- CI/CD ready

---

# Current Status

| Service | Status | Documentation | Last Updated |
|----------|--------|---------------|--------------|
| Compound Catalog API | ✅ Implemented | [README](./Algocura.CompoundCatalog.Api/README.md) | 2026-01-04 |
| Screening API | 🚧 Planned | — | — |
| AI Discovery API | 🚧 Planned | — | — |
| Assay Management API | 🚧 Planned | — | — |
| Workflow API | 🚧 Planned | — | — |
| Notification API | 🚧 Planned | — | — |
| Reporting API | 🚧 Planned | — | — |
| Search API | 🚧 Planned | — | — |
| Audit API | 🚧 Planned | — | — |
| Identity API | 🚧 Planned | — | — |