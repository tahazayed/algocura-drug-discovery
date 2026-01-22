# Authorization Role Matrix

The controller authorization attributes use the seeded role names:

- `Administrator`
- `Scientist`
- `Reviewer`
- `Service`

## Rules

| Controller type | Allowed roles |
|---|---|
| Identity user administration | Administrator |
| Audit query/read API | Reviewer, Administrator |
| Synchronization/internal ingestion APIs | Service, Administrator |
| Notification rule administration | Administrator |
| Scientific/business APIs | Scientist, Reviewer, Administrator |
| Identity `/auth/me` | Any authenticated user |
| Identity login/register | Anonymous |

`Service` is intentionally restricted to synchronization/internal endpoints and
is not granted general access to scientific user-facing APIs.

These attributes are additive to JWT authentication registered through
`Algocura.Platform.Security`.
