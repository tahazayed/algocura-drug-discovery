# FR-12: Algocura.Search.Api — BMAD feature brief

**Status:** Draft, source-derived. **Technical evidence:** [`specs/Algocura.Search.Api/spec.md`](../../specs/Algocura.Search.Api/spec.md)

## Documented purpose and capabilities

# Algocura Search API

The Search API provides a unified read model across the Algocura Drug
Discovery Platform.

It consumes supported integration events, converts them into searchable
documents, maintains local projections, and exposes filtering and
free-text search endpoints.

## Observed HTTP endpoints (where explicitly present in technical spec)
- `GET /api/v1/search`
- `GET /api/v1/search-documents/{id}`
- `POST /api/v1/search-documents/{id}/archive`
- `POST /api/v1/search-documents/{id}/restore`
- `POST /api/v1/search/synchronization`

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
