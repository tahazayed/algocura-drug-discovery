# FR-08: Algocura.LiteratureIntelligence.Api — BMAD feature brief

**Status:** Draft, source-derived. **Technical evidence:** [`specs/Algocura.LiteratureIntelligence.Api/spec.md`](../../specs/Algocura.LiteratureIntelligence.Api/spec.md)

## Documented purpose and capabilities

# Algocura Literature Intelligence API

The service starts the Algocura discovery workflow:

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

It registers publications, extracts reviewable disease-gene evidence,
supports human validation, creates target hypotheses, and publishes
integration events for downstream services.

## Observed HTTP endpoints (where explicitly present in technical spec)
- `GET /api/v1/publications`
- `GET /api/v1/publications/{id}`
- `POST /api/v1/publications`
- `POST /api/v1/publications/{id}/analyze`
- `DELETE /api/v1/publications/{id}`
- `GET /api/v1/evidence`
- `POST /api/v1/evidence/{id}/validate`
- `GET /api/v1/target-hypotheses`
- `GET /api/v1/target-hypotheses/{id}`
- `POST /api/v1/target-hypotheses`
- `POST /api/v1/target-hypotheses/{id}/publish`
- `POST /api/v1/literature-intelligence/synchronization`

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
