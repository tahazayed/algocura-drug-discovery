# FR-02: Algocura.AssayManagement.Api — BMAD feature brief

**Status:** Draft, source-derived. **Technical evidence:** [`specs/Algocura.AssayManagement.Api/spec.md`](../../specs/Algocura.AssayManagement.Api/spec.md)

## Documented purpose and capabilities

# Algocura Assay Management API

The service extends the Algocura discovery workflow:

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

It consumes completed-screening snapshots, manages assays and measurements,
calculates compound-level assay results, applies quality-control state,
publishes assay snapshots, and supports downstream synchronization.

## Observed HTTP endpoints (where explicitly present in technical spec)
- `GET /api/v1/completed-screenings`
- `GET /api/v1/completed-screenings/{id}`
- `GET /api/v1/assays`
- `GET /api/v1/assays/{id}`
- `POST /api/v1/assays`
- `POST /api/v1/assays/{id}/measurements`
- `POST /api/v1/assays/{id}/complete`
- `POST /api/v1/assays/{id}/publish`
- `POST /api/v1/assay-management/synchronization`
- `POST /api/v1/assay-management/synchronization/{id}`

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
