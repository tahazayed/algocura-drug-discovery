# BMAD requirements clarification register

All items require a human decision or further source-level verification.

1. Who are the product owner, scientific approvers, and primary user roles?
2. What are the supported end-to-end research workflows and success/failure outcomes?
3. What exact domain formulas, units, thresholds, rounding rules, and score interpretations are intended?
4. What input validation, error contracts, and authorization rules apply to every operation?
5. What are the event schema versions, idempotency rules, retry policies, and ordering guarantees?
6. What database constraints, migrations, tenancy, retention, and deletion rules are required?
7. What frontend screens, interactions, responsive breakpoints, and accessibility criteria must be reproduced?
8. What scientific validity disclaimers and human review gates are required?
9. Which integrations need live credentials, sandbox mocks, or replacement providers?
10. What measured acceptance thresholds apply to security, latency, throughput, and availability?
11. What original test fixtures and baseline snapshots must be preserved for parity?
12. Which existing behaviors are defects to fix versus intentional behaviors to reproduce?

Also review `../OPEN_QUESTIONS.md`.
