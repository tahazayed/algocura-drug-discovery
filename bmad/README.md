# BMAD analysis package — Algocura rebuild

This folder adds **BMAD-oriented draft artifacts** to the existing source-derived technical specifications. They are NOT BMAD tool-generated or stakeholder-approved outputs. The BMAD Method itself is **not bundled or installed**.

## Recommended BMAD sequence
1. Review `product-brief.md` with scientific and product stakeholders.
2. Resolve `requirements-questions.md` and root `OPEN_QUESTIONS.md`.
3. Run BMAD `bmad-create-prd` using the brief and source-derived technical specs as input; reconcile with `prd.md`.
4. Run `bmad-validate-prd`; mark unresolved items explicitly.
5. Feed approved PRD features into Spec-Kit `/speckit-specify`, `/speckit-clarify`, `/speckit-plan`, and `/speckit-tasks`.
6. Implement with Superpowers using TDD; preserve original repository for parity testing.

## Files
- `product-brief.md`: observed product scope and intended users (inferences labeled)
- `prd.md`: requirements inventory, traceability, constraints, and release criteria
- `features/`: one business-level feature brief per backend API
- `requirements-questions.md`: decisions requiring human approval
- `prompts/`: BMAD agent handoff prompts

No claim of complete behavioral reconstruction is made.
