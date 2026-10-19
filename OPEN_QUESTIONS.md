# Decisions required before a truly complete greenfield rebuild

1. Is the goal **behavioral parity** with the current repository, or an improved product? These are different specifications.
2. Which repository behavior is authoritative when README, implementation, and tests disagree?
3. What are the approved domain workflows, scientific methods, and acceptance criteria for each feature?
4. What is the authoritative database schema, migration history, and seed/reference data?
5. What are the exact API error formats, status codes, DTO examples, and backward-compatibility requirements?
6. What are the permissions and tenant/data isolation requirements for every operation?
7. What are the contracts for external data sources, ML models, messaging, retries, and failure recovery?
8. Which UI states, accessibility requirements, responsive layouts, and design files are authoritative?
9. Which performance, availability, observability, security and deployment targets are required?
10. Which functionality is prototype-only, incomplete, or intentionally stubbed in the existing code?

These cannot all be settled from source code alone. Do not treat inferred behavior as confirmed product requirements.
