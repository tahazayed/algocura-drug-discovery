# FR-14: Algocura.Workflow.Api — BMAD feature brief

**Status:** Draft, source-derived. **Technical evidence:** [`specs/Algocura.Workflow.Api/spec.md`](../../specs/Algocura.Workflow.Api/spec.md)

## Documented purpose and capabilities

# Algocura Workflow API

The Workflow API tracks and coordinates the end-to-end discovery process:

```text
Literature Intelligence
        ↓
Target Discovery
        ↓
Chemical Design
        ↓
Compound Catalog
        ↓
Screening
        ↓
Assay Management
        ↓
AI Discovery
```

The service owns workflow state only. It does not directly access another
service's database and does not perform scientific calculations.

## Observed HTTP endpoints (where explicitly present in technical spec)
- `GET /api/v1/workflows`
- `GET /api/v1/workflows/{id}`
- `POST /api/v1/workflows`
- `POST /api/v1/workflows/{id}/start`
- `POST /api/v1/workflows/{id}/stages/{stage}/fail`
- `POST /api/v1/workflows/{id}/cancel`
- `POST /api/v1/workflow/synchronization`
- `POST /api/v1/workflow/synchronization/{id}`

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
