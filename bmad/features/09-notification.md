# FR-09: Algocura.Notification.Api — BMAD feature brief

**Status:** Draft, source-derived. **Technical evidence:** [`specs/Algocura.Notification.Api/spec.md`](../../specs/Algocura.Notification.Api/spec.md)

## Documented purpose and capabilities

# Algocura Notification API

The Notification API provides cross-cutting notification management for
the Algocura Drug Discovery Platform.

It subscribes to `algocura-events`, applies configurable notification
rules, creates delivery records, dispatches notifications through a
prototype provider, records delivery attempts, and publishes delivery
status events.

## Observed HTTP endpoints (where explicitly present in technical spec)
- `GET /api/v1/notification-rules`
- `GET /api/v1/notification-rules/{id}`
- `POST /api/v1/notification-rules`
- `PUT /api/v1/notification-rules/{id}`
- `DELETE /api/v1/notification-rules/{id}`
- `GET /api/v1/notifications`
- `GET /api/v1/notifications/{id}`
- `POST /api/v1/notifications`
- `POST /api/v1/notifications/{id}/dispatch`
- `POST /api/v1/notifications/{id}/cancel`
- `POST /api/v1/notification/synchronization`

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
