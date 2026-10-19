# FR-07: Algocura.Identity.Api — BMAD feature brief

**Status:** Draft, source-derived. **Technical evidence:** [`specs/Algocura.Identity.Api/spec.md`](../../specs/Algocura.Identity.Api/spec.md)

## Documented purpose and capabilities

# Algocura Identity API

The Identity API provides authentication, user management, role management,
and JWT bearer tokens for the Algocura Drug Discovery Platform.

## Observed HTTP endpoints (where explicitly present in technical spec)
See the linked technical specification and route index; no endpoints extracted from this section.

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
