# Algocura Drug Discovery Platform

The **Algocura Drug Discovery Platform** is a cloud-native, event-driven microservices platform designed to support an end-to-end computational drug discovery workflow.

The platform begins with scientific literature analysis and target hypothesis generation, continues through target discovery and chemical design, and then proceeds to compound registration, screening, and AI-assisted candidate prioritization. Literature Intelligence, Target Discovery, Chemical Design, Compound Catalog, Screening, and AI Discovery are currently implemented.

Each microservice owns its data and communicates asynchronously through Azure Service Bus while exposing REST APIs for synchronous operations.

---

# Documentation

This repository contains multiple independently deployable microservices. Each service maintains its own README containing implementation details, API documentation, configuration, and local development instructions.

| Service | Documentation |
|----------|---------------|
| Literature Intelligence API | [Algocura.LiteratureIntelligence.Api](./Algocura.LiteratureIntelligence.Api/README.md) |
| Target Discovery API | [Algocura.TargetDiscovery.Api](./Algocura.TargetDiscovery.Api/README.md) |
| Chemical Design API | [Algocura.ChemicalDesign.Api](./Algocura.ChemicalDesign.Api/README.md) |
| Compound Catalog API | [Algocura.CompoundCatalog.Api](./Algocura.CompoundCatalog.Api/README.md) |
| Screening API | [Algocura.Screening.Api](./Algocura.Screening.Api/README.md) |
| AI Discovery API | [Algocura.AIDiscovery.Api](./Algocura.AIDiscovery.Api/README.md) |
| Assay Management API | Coming Soon |
| Workflow API | Coming Soon |
| Notification API | Coming Soon |
| Reporting API | Coming Soon |
| Search API | Coming Soon |
| Audit API | Coming Soon |
| Identity API | Coming Soon |

---

# Discovery Workflow

```text
Literature Intelligence API
        ↓
Target Discovery API
        ↓
Chemical Design API
        ↓
Compound Catalog API
        ↓
Screening API
        ↓
AI Discovery API
```

The workflow is designed to support the following progression:

1. Register and analyze scientific publications.
2. Extract disease-gene evidence.
3. Validate evidence and publish target hypotheses.
4. Evaluate and select biological targets.
5. Identify chemical substructures, scaffolds, and candidate structures.
6. Register candidate compounds.
7. Execute virtual or experimental screening workflows.
8. Rank and prioritize lead compounds using AI-assisted analysis.

---

# Solution Architecture

```text
                                      +----------------------+
                                      |      API Gateway     |
                                      | (Azure APIM / YARP)  |
                                      +----------+-----------+
                                                 |
        ---------------------------------------------------------------------------------------------------------
        |                    |                    |                    |                    |                    |
        v                    v                    v                    v                    v                    v

+--------------------+ +--------------------+ +--------------------+ +--------------------+ +----------------+ +----------------+
| Literature         | | Target             | | Chemical           | | Compound           | | Screening      | | AI Discovery   |
| Intelligence API   | | Discovery API      | | Design API         | | Catalog API        | | API            | | API            |
+---------+----------+ +---------+----------+ +---------+----------+ +---------+----------+ +--------+-------+ +--------+-------+
          |                      |                      |                      |                     |                  |
          ----------------------------------------------------------------------------------------------------------
                                                         |
                                                         v
                                             +--------------------------+
                                             |    Azure Service Bus     |
                                             |  Topic: algocura-events  |
                                             +------------+-------------+
                                                          |
        ---------------------------------------------------------------------------------------------------------
        |                    |                    |                    |                    |                    |
        v                    v                    v                    v                    v                    v

+--------------------+ +--------------------+ +--------------------+ +--------------------+ +----------------+ +----------------+
| Assay              | | Workflow           | | Notification       | | Reporting          | | Search         | | Audit          |
| Management API     | | API                | | API                | | API                | | API            | | API            |
+--------------------+ +--------------------+ +--------------------+ +--------------------+ +----------------+ +----------------+

                                                          |
                                                          v
                                      +--------------------------------------+
                                      |        Infrastructure Layer          |
                                      +--------------------------------------+
                                      | SQL Server (database per service)    |
                                      | Entity Framework Core                |
                                      | Azure Blob Storage                   |
                                      | Azure Service Bus Emulator           |
                                      | Docker Compose                       |
                                      +--------------------------------------+
```

---

# Microservices

| Service | Responsibility | Documentation |
|----------|----------------|---------------|
| Literature Intelligence API | Scientific publication management, disease-gene evidence extraction, human validation, target hypothesis creation, and synchronization | [README](./Algocura.LiteratureIntelligence.Api/README.md) |
| Target Discovery API | Literature-derived target synchronization, target assessment, druggability evaluation, target selection, and downstream synchronization | [README](./Algocura.TargetDiscovery.Api/README.md) |
| Chemical Design API | Selected-target synchronization, design projects, query structures, prototype structure search, candidate selection, synthesis planning, and compound-registration handoff | [README](./Algocura.ChemicalDesign.Api/README.md) |
| Compound Catalog API | Master repository for chemical compounds and lifecycle management | [README](./Algocura.CompoundCatalog.Api/README.md) |
| Screening API | Virtual screening, candidate evaluation, ranking, and synchronization | [README](./Algocura.Screening.Api/README.md) |
| AI Discovery API | AI-assisted compound prioritization, prediction models, screening projection synchronization, and lead ranking | [README](./Algocura.AIDiscovery.Api/README.md) |
| Assay Management API | Biological assay management | Coming Soon |
| Workflow API | Drug discovery workflow orchestration | Coming Soon |
| Search API | Compound and experiment search | Coming Soon |
| Notification API | Email, Teams, and event notifications | Coming Soon |
| Reporting API | Analytics and reporting | Coming Soon |
| Audit API | Audit trail and compliance | Coming Soon |
| Identity API | Authentication and authorization | Coming Soon |

---

# Event-Driven Architecture

The platform follows an event-driven architecture using Azure Service Bus.

```text
Literature Intelligence API
    │
    ├── PublicationRegistered
    ├── LiteratureEvidenceDiscovered
    ├── TargetHypothesisProposed
    └── LiteratureIntelligenceSnapshot
             │
             ▼
      Azure Service Bus
             │
             ▼
      Target Discovery API
             │
             ├── Consumes LiteratureIntelligenceSnapshot
             ├── Synchronizes target hypotheses
             ├── Maintains local target projections
             ├── Creates target assessments
             ├── Evaluates genetic evidence
             ├── Evaluates biological plausibility
             ├── Evaluates druggability
             ├── Evaluates safety risk
             ├── Evaluates translational potential
             ├── Publishes TargetAssessmentCreated
             ├── Publishes TargetAssessmentCompleted
             ├── Publishes TargetSelected
             └── Publishes TargetDiscoverySnapshot
                        │
                        ▼
                 Azure Service Bus
                        │
                        ▼
                Chemical Design API
                        │
                        ├── Consumes TargetDiscoverySnapshot
                        ├── Synchronizes selected targets
                        ├── Maintains local target projections
                        ├── Creates chemical design projects
                        ├── Stores query structures
                        ├── Executes prototype exact, substructure, and similarity searches
                        ├── Ranks candidate structures
                        ├── Records synthesis routes and reaction steps
                        ├── Publishes ChemicalDesignProjectCreated
                        ├── Publishes StructureSearchCompleted
                        ├── Publishes CandidateSelectedForRegistration
                        └── Publishes ChemicalDesignSnapshot
                                   │
                                   ▼
                            Azure Service Bus
                                   │
                                   ▼
                          Compound Catalog API
                                   │
                                   ├── CompoundRegistered
                                   ├── CompoundUpdated
                                   ├── CompoundDeleted
                                   ├── CompoundStatusChanged
                                   └── CompoundCatalogSnapshot
                                              │
                                              ▼
                                       Azure Service Bus
                                              │
                                              ▼
                                      Screening API
                                              │
                                              ├── Synchronizes compound catalog
                                              ├── Maintains local compound projection
                                              ├── Executes screening workflows
                                              ├── ScreeningCompleted
                                              └── CompletedScreeningSnapshot
                                                         │
                                                         ▼
                                                  Azure Service Bus
                                                         │
                                                         ▼
                                                AI Discovery API
                                                         │
                                                         ├── Synchronizes compound projections
                                                         ├── Synchronizes completed screenings
                                                         ├── Executes prediction models
                                                         ├── Ranks lead compounds
                                                         └── Produces prediction results
```

Synchronization endpoints allow downstream services to rebuild local projections after deployment, database recreation, or subscription loss without introducing direct database dependencies.

---

# Literature Intelligence Workflow

The Literature Intelligence API currently supports:

```text
Register publication
        ↓
Analyze title and abstract
        ↓
Create disease-gene evidence
        ↓
Human validation
        ↓
Create target hypothesis
        ↓
Publish target hypothesis
        ↓
Target Discovery API
```

The current literature analysis implementation is a deterministic prototype. It detects supplied gene symbols in publication text and creates evidence records for scientific review.

It does not independently establish biological causality and does not replace expert validation.

---

# Target Discovery Workflow

The Target Discovery API currently supports:

```text
Consume LiteratureIntelligenceSnapshot
        ↓
Synchronize target hypothesis
        ↓
Create target assessment
        ↓
Evaluate evidence and druggability
        ↓
Complete assessment
        ↓
Select target
        ↓
Publish TargetDiscoverySnapshot
        ↓
Chemical Design API
```

The current target assessment engine is a deterministic prototype. It combines literature evidence, genetic evidence, biological plausibility, druggability, safety risk, and translational potential into a normalized assessment score.

It does not replace expert scientific review, experimental validation, or formal safety assessment.

---

# Chemical Design Workflow

The Chemical Design API currently supports:

```text
Consume TargetDiscoverySnapshot
        ↓
Synchronize selected target
        ↓
Create chemical design project
        ↓
Add query structure
        ↓
Run prototype structure search
        ↓
Rank candidate structures
        ↓
Record synthesis routes and reaction steps
        ↓
Select candidate for registration
        ↓
Publish ChemicalDesignSnapshot
        ↓
Compound Catalog API
```

The current structure-search engine is a deterministic prototype. It supports exact, substructure-like, and similarity-like search modes using normalized structure strings and token comparison.

It is not ChemAxon and is not a chemically validated search engine. It must be replaced with a ChemAxon integration or another validated cheminformatics engine before scientific use.

---

# Data Ownership

Each microservice owns its own database.

```text
Literature Intelligence API
    SQL Server

Target Discovery API
    SQL Server

Chemical Design API
    SQL Server

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

Services maintain only the local projections required for their responsibilities.

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
| API Gateway | Azure API Management / YARP |
| Containers | Docker |
| Orchestration | Kubernetes (planned) |
| API Documentation | OpenAPI, Swagger UI, ReDoc |
| Testing | xUnit, FluentAssertions, NSubstitute, SQLite |
| Logging | Microsoft.Extensions.Logging |
| Health Monitoring | ASP.NET Core Health Checks |
| Literature Analysis | Deterministic prototype with future biomedical NLP integration |
| Chemical Structure Search | Deterministic prototype with future ChemAxon or validated cheminformatics integration |
| AI Inference | Deterministic prototype with future trained-model integration |

---

# Solution Structure

```text
Algocura.DrugDiscovery
│
├── README.md
├── compose.yaml
│
├── Algocura.LiteratureIntelligence.Api
│   └── README.md
│
├── Algocura.TargetDiscovery.Api
│   └── README.md
│
├── Algocura.ChemicalDesign.Api
│   └── README.md
│
├── Algocura.CompoundCatalog.Api
│   └── README.md
│
├── Algocura.Screening.Api
│   └── README.md
│
├── Algocura.AIDiscovery.Api
│   └── README.md
│
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

Local development uses Docker Compose to provision the following infrastructure and services.

## Infrastructure Components

- SQL Server
- Azure Service Bus Emulator
- Azurite Storage Emulator

## Implemented Microservices

- Literature Intelligence API
- Target Discovery API
- Chemical Design API
- Compound Catalog API
- Screening API
- AI Discovery API

Additional microservices will be added to the Docker Compose environment as they are implemented.

## Azure Service Bus Topology

### Topic

```text
algocura-events
```

### Subscriptions

```text
target-discovery-api
chemical-design-api
compound-catalog-api
screening-api
ai-discovery-api
```

Each consuming service owns a dedicated subscription. This allows services to process events independently while sharing the same integration-event topic.

The `target-discovery-api` subscription is used by the Target Discovery API, and the `chemical-design-api` subscription is used by the implemented Chemical Design API.

---

# Current Platform Capabilities

The platform currently supports:

- Scientific publication registration
- Publication author metadata
- Literature search and filtering
- Disease-gene evidence extraction
- Human evidence validation
- Target hypothesis creation
- Target hypothesis publication and synchronization
- Literature target projection synchronization
- Target assessment creation and update
- Genetic evidence scoring
- Biological plausibility scoring
- Druggability scoring
- Safety-risk scoring
- Translational-potential scoring
- Target assessment completion
- Target selection
- Selected-target synchronization
- Chemical design project management
- Query structure management
- Prototype exact, substructure, and similarity search
- Candidate structure ranking
- Candidate selection for registration
- Synthesis-route management
- Ordered reaction-step management
- Chemical design synchronization
- Compound-registration handoff events
- Compound lifecycle management
- Compound search and filtering
- Compound synchronization across services
- Virtual screening management
- Screening candidate ranking
- Completed screening synchronization
- AI-assisted prototype prediction workflows
- Candidate scoring and prioritization
- OpenAPI documentation
- Health endpoints
- SQL Server persistence
- Docker-based local development
- Azure Service Bus Emulator integration
- Automated unit testing

---

# Design Principles

- Microservices architecture
- Domain-Driven Design
- Event-driven communication
- Database per service
- Local projection ownership
- Idempotent event consumption
- Explicit synchronization endpoints
- RESTful APIs
- OpenAPI-first development
- Containerized deployment
- Cloud-native architecture
- Observability and structured logging
- Health monitoring
- Automated testing
- Human validation of scientific evidence
- CI/CD readiness

---

# Current Status

| Service | Responsibility | Status | Documentation | Last Updated |
|----------|----------------|--------|---------------|--------------|
| Literature Intelligence API | Publication management, disease-gene evidence extraction, target hypotheses, and synchronization | ✅ Implemented | [README](./Algocura.LiteratureIntelligence.Api/README.md) | 2026-01-11 |
| Target Discovery API | Literature target synchronization, target assessment, druggability evaluation, target selection, and synchronization | ✅ Implemented | [README](./Algocura.TargetDiscovery.Api/README.md) | 2026-01-11 |
| Chemical Design API | Selected-target synchronization, design projects, structure search, candidate selection, synthesis planning, and compound-registration handoff | ✅ Implemented | [README](./Algocura.ChemicalDesign.Api/README.md) | 2026-01-12 |
| Compound Catalog API | Master repository for compounds and lifecycle management | ✅ Implemented | [README](./Algocura.CompoundCatalog.Api/README.md) | 2026-01-04 |
| Screening API | Virtual screening, candidate evaluation, ranking, and synchronization | ✅ Implemented | [README](./Algocura.Screening.Api/README.md) | 2026-01-09 |
| AI Discovery API | Prediction workflows, candidate prioritization, and lead ranking | ✅ Implemented | [README](./Algocura.AIDiscovery.Api/README.md) | 2026-01-10 |
| Assay Management API | Biological assay management | 🚧 Planned | — | — |
| Workflow API | Drug discovery workflow orchestration | 🚧 Planned | — | — |
| Notification API | Email, Teams, and event notifications | 🚧 Planned | — | — |
| Reporting API | Analytics and reporting | 🚧 Planned | — | — |
| Search API | Compound and experiment search | 🚧 Planned | — | — |
| Audit API | Audit trail and compliance | 🚧 Planned | — | — |
| Identity API | Authentication and authorization | 🚧 Planned | — | — |
