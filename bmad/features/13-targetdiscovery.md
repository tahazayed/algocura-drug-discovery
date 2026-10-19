# FR-13: Algocura.TargetDiscovery.Api — BMAD feature brief

**Status:** Draft, source-derived. **Technical evidence:** [`specs/Algocura.TargetDiscovery.Api/spec.md`](../../specs/Algocura.TargetDiscovery.Api/spec.md)

## Documented purpose and capabilities

# Algocura Target Discovery API

The **Algocura Target Discovery API** is the second service in the
Algocura discovery workflow:

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

The service consumes literature-derived target hypotheses, maintains
local projections, manages target assessments, calculates prototype
assessment scores, selects targets, and publishes selected-target
snapshots for the Chemical Design API.

## Observed HTTP endpoints (where explicitly present in technical spec)
- `GET /api/v1/target-hypotheses`
- `GET /api/v1/target-hypotheses/{id}`
- `GET /api/v1/target-assessments`
- `GET /api/v1/target-assessments/{id}`
- `POST /api/v1/target-assessments`
- `PUT /api/v1/target-assessments/{id}`
- `POST /api/v1/target-assessments/{id}/complete`
- `POST /api/v1/target-assessments/{id}/select`
- `DELETE /api/v1/target-assessments/{id}`
- `POST /api/v1/target-discovery/synchronization`
- `POST /api/v1/target-discovery/synchronization/{id}`

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
