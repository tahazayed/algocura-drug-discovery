# FR-04: Algocura.ChemicalDesign.Api — BMAD feature brief

**Status:** Draft, source-derived. **Technical evidence:** [`specs/Algocura.ChemicalDesign.Api/spec.md`](../../specs/Algocura.ChemicalDesign.Api/spec.md)

## Documented purpose and capabilities

# Algocura Chemical Design API

The service is the third stage in the workflow:

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

It consumes `TargetDiscoverySnapshot`, manages design projects and query
structures, executes prototype structure searches, ranks candidate
structures, stores synthesis routes, and publishes selected compounds.

## Observed HTTP endpoints (where explicitly present in technical spec)
- `GET /api/v1/selected-targets`
- `GET /api/v1/selected-targets/{id}`
- `POST /api/v1/design-projects`
- `GET /api/v1/design-projects/{id}`
- `POST /api/v1/design-projects/{id}/query-structures`
- `POST /api/v1/design-projects/{projectId}/structure-searches`
- `GET /api/v1/structure-searches/{id}`
- `POST /api/v1/structure-searches/{id}/run`
- `GET /api/v1/candidates/{id}`
- `POST /api/v1/candidates/{id}/select-for-registration`
- `POST /api/v1/candidates/{id}/synthesis-routes`
- `POST /api/v1/chemical-design/synchronization`
- `POST /api/v1/chemical-design/synchronization/{id}`

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
