# Algocura — Product Requirements Document (BMAD working draft)

**Status:** NOT APPROVED. Extracted from repository documentation and technical inventory. This is a requirements baseline, not a verified full functional specification.

## 1. Product summary
Reconstruct the modular Algocura drug-discovery platform, preserving its documented service boundaries, API contracts, data structures, event flows, UI routes, and automated tests. See `../architecture/project-map.md`, `../architecture/route-index.md`, and `../architecture/frontend-contracts.md`.

## 2. Stakeholders and personas
**Unconfirmed:** discovery scientists, chemists, assay researchers, clinical researchers, admins, reviewers. Obtain stakeholder approval of role-to-workflow mapping.

## 3. Functional requirements inventory
Each FR below is an **observed module-level capability**, not a complete statement of its edge-case behavior. The linked technical spec is the evidence source; the corresponding `features/*.md` contains more detail and unresolved questions.

| ID | Domain | Evidence | API endpoint mentions |
|---|---|---|---|
| 01 | Algocura.AIDiscovery.Api | [Technical spec](../../specs/Algocura.AIDiscovery.Api/spec.md) | 0 documented endpoint mentions |
| 02 | Algocura.AssayManagement.Api | [Technical spec](../../specs/Algocura.AssayManagement.Api/spec.md) | 10 documented endpoint mentions |
| 03 | Algocura.Audit.Api | [Technical spec](../../specs/Algocura.Audit.Api/spec.md) | 0 documented endpoint mentions |
| 04 | Algocura.ChemicalDesign.Api | [Technical spec](../../specs/Algocura.ChemicalDesign.Api/spec.md) | 13 documented endpoint mentions |
| 05 | Algocura.ClinicalResearch.Api | [Technical spec](../../specs/Algocura.ClinicalResearch.Api/spec.md) | 7 documented endpoint mentions |
| 06 | Algocura.CompoundCatalog.Api | [Technical spec](../../specs/Algocura.CompoundCatalog.Api/spec.md) | 0 documented endpoint mentions |
| 07 | Algocura.Identity.Api | [Technical spec](../../specs/Algocura.Identity.Api/spec.md) | 0 documented endpoint mentions |
| 08 | Algocura.LiteratureIntelligence.Api | [Technical spec](../../specs/Algocura.LiteratureIntelligence.Api/spec.md) | 12 documented endpoint mentions |
| 09 | Algocura.Notification.Api | [Technical spec](../../specs/Algocura.Notification.Api/spec.md) | 11 documented endpoint mentions |
| 10 | Algocura.Reporting.Api | [Technical spec](../../specs/Algocura.Reporting.Api/spec.md) | 0 documented endpoint mentions |
| 11 | Algocura.Screening.Api | [Technical spec](../../specs/Algocura.Screening.Api/spec.md) | 0 documented endpoint mentions |
| 12 | Algocura.Search.Api | [Technical spec](../../specs/Algocura.Search.Api/spec.md) | 5 documented endpoint mentions |
| 13 | Algocura.TargetDiscovery.Api | [Technical spec](../../specs/Algocura.TargetDiscovery.Api/spec.md) | 11 documented endpoint mentions |
| 14 | Algocura.Workflow.Api | [Technical spec](../../specs/Algocura.Workflow.Api/spec.md) | 8 documented endpoint mentions |

## 4. Cross-cutting requirements
- **Identity and authorization:** retain observed security mechanisms; verify actual permission matrices against `AUTHORIZATION.md` and service contracts.
- **Audit:** retain the observed audit API and shared auditing project; approve retention and integrity rules.
- **Messaging:** retain documented event publishers, consumers, and message schemas; resolve delivery/retry/idempotency requirements.
- **Data:** preserve documented entity fields and relationships; approve migration strategy, constraints, and data retention.
- **Frontend:** recreate routes, forms, validations, state transitions, accessibility, and API bindings based on `../architecture/frontend-contracts.md`; inventory alone does not establish pixel-level fidelity.
- **Testing:** reimplement documented test scenarios and add acceptance tests for confirmed business rules.

## 5. Nonfunctional requirements — pending decisions
Performance SLOs, scale, disaster recovery, observability, secrets handling, regulatory posture, accessibility level, browser support, and supported environments require stakeholder decisions. Do not invent thresholds.

## 6. Acceptance and parity gate
A feature is accepted only when its approved user story and examples are implemented, API and event contract tests pass, persistence and authorization behavior are verified, and relevant end-to-end workflows pass. Rebuilt outputs should be compared with an archived reference implementation for behavioral parity.

## 7. Risks and unresolved decisions
See `requirements-questions.md` and `../OPEN_QUESTIONS.md`. Scientific scores are prototypes unless independently validated; do not claim clinical or predictive validity.

## 8. Approval record
Product owner: **TBD**. Scientific owner: **TBD**. Security reviewer: **TBD**. Approval date: **TBD**.
