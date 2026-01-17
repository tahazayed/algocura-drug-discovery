# Algocura Drug Discovery Platform

The **Algocura Drug Discovery Platform** is a cloud-native, event-driven microservices platform designed to support an end-to-end computational drug discovery workflow.

The platform begins with scientific literature analysis and target hypothesis generation, continues through target discovery and chemical design, and then proceeds to compound registration, screening, and AI-assisted candidate prioritization. Literature Intelligence, Target Discovery, Chemical Design, Compound Catalog, Screening, Assay Management, AI Discovery, Workflow, Notification, Search, Reporting, and Audit are currently implemented.

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
| Assay Management API | [Algocura.AssayManagement.Api](./Algocura.AssayManagement.Api/README.md) |
| Workflow API | [Algocura.Workflow.Api](./Algocura.Workflow.Api/README.md) |
| Notification API | [Algocura.Notification.Api](./Algocura.Notification.Api/README.md) |
| Reporting API | [Algocura.Reporting.Api](./Algocura.Reporting.Api/README.md) |
| Search API | [Algocura.Search.Api](./Algocura.Search.Api/README.md) |
| Audit API | [Algocura.Audit.Api](./Algocura.Audit.Api/README.md) |
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
Assay Management API
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
8. Create and execute biological or analytical assays.
9. Capture measurements, replicates, quality-control data, and compound-level results.
10. Rank and prioritize lead compounds using AI-assisted analysis.

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
| Assay Management API | Completed-screening synchronization, assay creation, measurements, replicates, quality control, result calculation, publication, and synchronization | [README](./Algocura.AssayManagement.Api/README.md) |
| Workflow API | End-to-end workflow creation, stage tracking, event correlation, orchestration state, failure handling, cancellation, idempotent event processing, and synchronization | [README](./Algocura.Workflow.Api/README.md) |
| Search API | Event-driven unified search projections, metadata filtering, free-text search, document lifecycle management, and index synchronization | [README](./Algocura.Search.Api/README.md) |
| Notification API | Event-driven notification rules, manual notifications, delivery attempts, dispatch state, cancellation, and synchronization | [README](./Algocura.Notification.Api/README.md) |
| Reporting API | Read-optimized reporting projections, report definitions, report execution, persisted metrics, platform summaries, discovery-funnel analytics, and reporting synchronization | [README](./Algocura.Reporting.Api/README.md) |
| Audit API | Centralized entity audit-event ingestion, immutable audit history, user/action traceability, correlation and workflow filtering, change-value inspection, summaries, and compliance-oriented querying | [README](./Algocura.Audit.Api/README.md) |
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
                                              ├── Publishes ScreeningCompleted
                                              └── Publishes CompletedScreeningSnapshot
                                                         │
                                                         ▼
                                                  Azure Service Bus
                                                         │
                                                         ▼
                                           Assay Management API
                                                         │
                                                         ├── Consumes CompletedScreeningSnapshot
                                                         ├── Synchronizes completed screenings
                                                         ├── Creates assays
                                                         ├── Records compound measurements and replicates
                                                         ├── Applies quality-control status
                                                         ├── Calculates compound-level assay results
                                                         ├── Publishes AssayCreated
                                                         ├── Publishes AssayCompleted
                                                         └── Publishes AssaySnapshot
                                                                    │
                                                                    ▼
                                                             Azure Service Bus
                                                                    │
                                                                    ▼
                                                           AI Discovery API
                                                                    │
                                                                    ├── Synchronizes compound projections
                                                                    ├── Synchronizes completed screenings
                                                                    ├── Consumes AssaySnapshot
                                                                    ├── Executes prediction models
                                                                    ├── Ranks lead compounds
                                                                    └── Produces prediction results

All platform services
        │
        │ Integration events with workflowId
        │ or workflow GUID as CorrelationId
        ▼
Azure Service Bus
        │
        ├───────────────────────────────────────────────┬───────────────────────────────────────────────┐
        ▼                                               ▼                                               ▼
Workflow API                                  Notification API                                  Search API
        │                                               │                                               │
        ├── Consumes platform snapshot                 ├── Consumes matching platform events          ├── Consumes supported platform events
        │   and completion events                      ├── Applies enabled notification rules         ├── Maps events to search-document types
        ├── Correlates events to workflows             ├── Creates notification records               ├── Creates or updates local projections
        ├── Tracks seven workflow stages               ├── Creates delivery records                   ├── Stores searchable metadata and tags
        ├── Stores processed event receipts            ├── Dispatches through configured channels    ├── Prevents duplicate entity projections
        ├── Prevents duplicate processing              ├── Records delivery attempts                  ├── Supports free-text and field filtering
        ├── Records external entity identifiers        ├── Tracks delivery and failure status         ├── Supports archive and restore
        ├── Tracks failures and cancellations          ├── Publishes NotificationDelivered            ├── Publishes SearchDocumentIndexed
        ├── Publishes WorkflowCreated                  ├── Publishes NotificationDeliveryFailed       └── Publishes SearchIndexSnapshot
        ├── Publishes WorkflowStageCompleted           └── Publishes NotificationSnapshot
        ├── Publishes WorkflowCompleted
        └── Publishes WorkflowSnapshot

Entity changes in audited services
        │
        │ EntityAuditEvent
        ▼
Azure Service Bus
        │
        ▼
Audit API
        │
        ├── Consumes EntityAuditEvent
        ├── Deduplicates by EventId
        ├── Preserves actor and change metadata
        ├── Stores previous/current JSON values
        ├── Supports service/entity/action/user filtering
        ├── Supports correlation/workflow/trace filtering
        ├── Exposes audit summaries
        └── Prevents audit-event feedback loops
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

# Assay Management Workflow

The Assay Management API currently supports:

```text
Consume CompletedScreeningSnapshot
        ↓
Synchronize completed screening
        ↓
Create assay
        ↓
Add compound measurements
        ↓
Record replicates and controls
        ↓
Apply quality-control status
        ↓
Calculate compound-level assay results
        ↓
Complete assay
        ↓
Publish AssaySnapshot
        ↓
AI Discovery API
```

The current assay-result calculator is a deterministic prototype. It calculates mean values, sample standard deviation, replicate count, a confidence score, and a simple activity outcome.

The data model includes IC50, EC50, Ki, and Kd fields, but the current implementation does not calculate scientifically validated dose-response or binding constants. Those calculations require validated analytical models, curve fitting, and experimental review.

---

# Workflow Orchestration

The Workflow API currently supports:

```text
Create discovery workflow
        ↓
Initialize seven workflow stages
        ↓
Start workflow
        ↓
Track Literature Intelligence
        ↓
Track Target Discovery
        ↓
Track Chemical Design
        ↓
Track Compound Catalog
        ↓
Track Screening
        ↓
Track Assay Management
        ↓
Track AI Discovery
        ↓
Complete workflow
```

The Workflow API owns orchestration state only. It does not directly call another service's database and does not perform scientific calculations.

Each workflow contains the following stages:

```text
LiteratureIntelligence
TargetDiscovery
ChemicalDesign
CompoundCatalog
Screening
AssayManagement
AiDiscovery
```

Each stage can be:

```text
Pending
InProgress
Completed
Failed
Skipped
```

Each workflow can be:

```text
Draft
InProgress
Completed
Failed
Cancelled
```

The Workflow API consumes platform events and advances the corresponding workflow stage. Incoming events must provide either a `workflowId` property in the payload or the workflow GUID as the Azure Service Bus `CorrelationId`.

Processed event IDs are stored in `WorkflowEventReceipts`, which provides idempotent event handling and prevents the same integration event from advancing a workflow more than once.

---

# Notification Workflow

The Notification API currently supports:

```text
Consume platform event
        ↓
Match enabled notification rules
        ↓
Create notification
        ↓
Create delivery record
        ↓
Dispatch notification
        ↓
Record delivery attempt
        ↓
Update delivered, partially delivered, or failed status
        ↓
Publish notification status event
```

The service can consume any event on `algocura-events` when an enabled rule matches its `eventType`.

Supported notification channels in the current model are:

```text
Email
MicrosoftTeams
Webhook
InApplication
```

The current `LoggingNotificationDispatcher` is a deterministic prototype. It records delivery activity in application logs and returns a simulated provider message identifier.

It does not send real email, Microsoft Teams messages, or webhooks. Production use requires approved provider integrations such as Microsoft Graph, Azure Communication Services, SendGrid, or Teams webhooks.

---

# Search Workflow

The Search API currently supports:

```text
Consume supported platform event
        ↓
Determine search-document type
        ↓
Extract searchable metadata
        ↓
Create or update local search projection
        ↓
Store tags and source identifiers
        ↓
Expose unified search and filtering
        ↓
Archive or restore search documents
        ↓
Publish SearchDocumentIndexed or SearchIndexSnapshot
```

The current implementation indexes the following document types:

```text
Publication
Target
ChemicalDesign
Compound
Screening
Assay
Prediction
Workflow
Notification
```

The search endpoint supports free-text search together with filters for document type, disease name, gene symbol, compound name, InChIKey, workflow identifier, document status, minimum score, and pagination.

The current implementation uses SQL Server projections and `Contains` filtering. It does not provide stemming, fuzzy matching, linguistic analysis, semantic ranking, vector search, or distributed indexing.

Production-scale search can use Azure AI Search, Elasticsearch, OpenSearch, or another approved search platform while preserving the current event-driven projection model and REST contract.

---

# Reporting Workflow

The Reporting API currently supports:

```text
Consume or synchronize platform projections
        ↓
Maintain read-optimized reporting projections
        ↓
Define reusable report definitions
        ↓
Execute report runs
        ↓
Calculate and persist report metrics
        ↓
Expose platform-summary and discovery-funnel analytics
        ↓
Publish ReportingSnapshot
```

The current implementation includes two prototype report types:

```text
PlatformSummary
DiscoveryFunnel
```

`PlatformSummary` reports summarize the reporting projections available across the platform, including completed workflow counts.

`DiscoveryFunnel` reports can filter by disease name and gene symbol and summarize progression through literature, selected targets, and prediction-stage candidates.

Report results are persisted as `ReportRun` and `ReportMetric` records. Reporting data remains read-optimized and isolated from operational service databases; the Reporting API does not query another microservice's database directly.

The current reporting engine is a deterministic prototype intended to validate platform architecture, aggregation, persistence, and API contracts. Production analytics can extend this model with additional KPIs, time-series aggregation, dashboards, export formats, and approved analytics platforms.


---

# Audit Workflow

The Audit API currently supports:

```text
EF Core entity change in a platform service
        ↓
Algocura.Platform.Auditing interceptor
        ↓
Create EntityAuditEvent
        ↓
Publish to Azure Service Bus
        ↓
Topic: algocura-events
        ↓
Subscription: audit-api
        ↓
Consume EntityAuditEvent
        ↓
Deduplicate by EventId
        ↓
Persist immutable audit record
        ↓
Expose audit search, detail, summary, and synchronization endpoints
```

The centralized audit record preserves:

```text
Event ID
Occurred timestamp
Service name
Entity type
Entity ID
Action
User ID
Subject
User display name
User email
Actor type
Previous values
Current values
Changed properties
Correlation ID
Workflow ID
Trace ID
IP address
User agent
Event type
Event version
Received timestamp
```

The Audit API is designed as a read-oriented compliance and traceability service. It does not modify source-domain entities and does not directly query another service's database.

Audit ingestion is idempotent. `EventId` is the primary key, so replaying the same `EntityAuditEvent` does not create duplicate history.

To avoid an audit feedback loop, the Audit API stamps its own persistence metadata but does not publish audit events for `AuditRecord` inserts.

For centralized audit history to receive live events, producing services must enable `PublishEntityAuditEvents` and use the shared Service Bus audit publisher.


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

Workflow API
    SQL Server

Notification API
    SQL Server

Search API
    SQL Server

Reporting API
    SQL Server

Audit API
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
| Assay Analysis | Deterministic result calculator with future validated dose-response and binding analysis |
| Workflow Orchestration | Event-correlated state machine with idempotent event receipts |
| Notification Delivery | Deterministic logging dispatcher with future provider-specific integrations |
| Search Indexing | SQL Server projections with future Azure AI Search, Elasticsearch, or OpenSearch integration |
| Reporting | SQL Server reporting projections, persisted report runs and metrics, with future dashboard/export integrations |
| Auditing | Algocura.Platform.Auditing, EF Core interceptors, EntityAuditEvent, Azure Service Bus, centralized SQL Server audit history |
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
│   └── README.md
│
├── Algocura.Workflow.Api
│   └── README.md
│
├── Algocura.Notification.Api
│   └── README.md
│
├── Algocura.Reporting.Api
│   └── README.md
│
├── Algocura.Search.Api
│   └── README.md
│
├── Algocura.Audit.Api
│   └── README.md
│
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
- Assay Management API
- AI Discovery API
- Workflow API
- Notification API
- Search API
- Reporting API
- Audit API

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
assay-management-api
ai-discovery-api
workflow-api
notification-api
search-api
reporting-api
audit-api
```

Each consuming service owns a dedicated subscription. This allows services to process events independently while sharing the same integration-event topic.

The `target-discovery-api`, `chemical-design-api`, `compound-catalog-api`, `screening-api`, `assay-management-api`, `ai-discovery-api`, `workflow-api`, `notification-api`, `search-api`, `reporting-api`, and `audit-api` subscriptions are used by their corresponding implemented consumers.

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
- Completed-screening projection synchronization
- Assay creation and lifecycle management
- Compound measurement capture
- Replicate tracking
- Assay quality-control status
- Z-prime factor storage
- Compound-level assay result calculation
- Mean and sample-standard-deviation calculation
- Assay outcome classification
- Assay publication and synchronization
- AssaySnapshot handoff to AI Discovery
- End-to-end discovery workflow creation
- Seven-stage workflow initialization
- Workflow start, cancellation, failure, and completion handling
- Workflow stage progression
- Event-to-workflow correlation
- Idempotent workflow event processing
- External entity tracking by workflow stage
- Workflow snapshot publication
- Workflow synchronization endpoints
- Event-driven notification rule management
- Notification template token replacement
- Manual notification creation
- Notification delivery tracking
- Delivery-attempt history
- Notification dispatch and retry state
- Notification cancellation
- Notification delivery status events
- Notification synchronization endpoints
- Event-driven unified search projections
- Search document creation and update
- Search document metadata and tag indexing
- Search projection deduplication
- Free-text search across indexed fields
- Search by document type
- Search by disease name
- Search by gene symbol
- Search by compound name
- Search by InChIKey
- Search by workflow identifier
- Search by minimum score
- Search document archive and restore
- Search index snapshot publication
- Search synchronization endpoints
- AI-assisted prototype prediction workflows
- Candidate scoring and prioritization
- Read-optimized reporting projections
- Report-definition management
- Platform-summary report execution
- Discovery-funnel report execution
- Persisted report-run history
- Persisted report metrics
- Reporting summary endpoint
- Reporting snapshot publication
- Reporting synchronization endpoint
- Centralized entity audit-event ingestion
- Idempotent audit event processing
- Audit search by service
- Audit search by entity type and entity ID
- Audit search by action
- Audit search by user ID
- Audit search by workflow and correlation identifiers
- Audit search by date range
- Previous-value and current-value inspection
- Changed-property inspection
- Actor, trace, IP-address, and user-agent metadata
- Audit summary aggregation
- Audit synchronization metadata
- Audit feedback-loop protection
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
- Workflow correlation using workflow identifiers
- Event-receipt deduplication
- Explicit synchronization endpoints
- RESTful APIs
- OpenAPI-first development
- Containerized deployment
- Cloud-native architecture
- Observability and structured logging
- Shared entity auditing and system-user attribution
- Centralized immutable audit history
- Audit-event deduplication
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
| Assay Management API | Completed-screening synchronization, assay measurements, quality control, result calculation, publication, and synchronization | ✅ Implemented | [README](./Algocura.AssayManagement.Api/README.md) | 2026-01-12 |
| Workflow API | End-to-end workflow orchestration, stage tracking, event correlation, idempotent processing, failure handling, cancellation, and synchronization | ✅ Implemented | [README](./Algocura.Workflow.Api/README.md) | 2026-01-13 |
| Notification API | Event-driven notification rules, manual notifications, delivery tracking, dispatch state, cancellation, and synchronization | ✅ Implemented | [README](./Algocura.Notification.Api/README.md) | 2026-01-13 |
| Reporting API | Reporting projections, report definitions, report execution, persisted metrics, platform summaries, discovery-funnel analytics, and synchronization | ✅ Implemented | [README](./Algocura.Reporting.Api/README.md) | 2026-01-16 |
| Search API | Event-driven unified search projections, metadata filtering, free-text search, document lifecycle management, and index synchronization | ✅ Implemented | [README](./Algocura.Search.Api/README.md) | 2026-01-13 |
| Audit API | Centralized audit-event ingestion, immutable audit history, actor/change traceability, filtering, summaries, and synchronization | ✅ Implemented | [README](./Algocura.Audit.Api/README.md) | 2026-01-16 |
| Identity API | Authentication and authorization | 🚧 Planned | — | — |
