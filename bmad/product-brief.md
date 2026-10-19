# Algocura Drug Discovery — Product Brief (BMAD draft)

**Status:** Draft inferred from source-derived documentation; requires stakeholder approval.

## Product concept
Algocura is a modular drug-discovery software platform. Its repository documents services for literature intelligence, target discovery, chemical design, compound catalog, screening, AI discovery, assay management, clinical research, workflow, identity, search, reporting, notifications, and audit, supported by shared platform libraries and a frontend.

## Observed discovery flow
The Target Discovery service README documents: Literature Intelligence → Target Discovery → Chemical Design → Compound Catalog → Screening → AI Discovery. This is a documented prototype workflow, not proof of scientific validation.

## Potential users (hypotheses; not confirmed)
Research scientists, medicinal chemists, assay scientists, discovery project managers, platform administrators, and compliance reviewers. Confirm actual personas and access boundaries.

## Product goals (proposed for validation)
- Support traceable research decisions across discovery stages.
- Provide consistent interfaces between domain services and frontend workflows.
- Retain authorization, auditability, and repeatable tests.
- Clearly distinguish prototype scores or predictions from scientifically validated results.

## Scope
Rebuild the modules listed in the solution project map while preserving documented contracts and behavior. Exact parity must be demonstrated against the archived reference implementation.

## Out of scope until approved
Clinical decision-making, validated predictive accuracy, regulatory compliance certification, and production-grade scientific conclusions are not established by the source inventory.

## Success criteria to define
Stakeholders must set measurable workflow completion, accuracy/quality, performance, security, availability, and data-integrity thresholds before sign-off.
