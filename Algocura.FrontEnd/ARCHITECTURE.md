# Frontend Architecture

```text
Browser
  |
  v
Algocura.DrugDiscovery.FrontEnd
  |
  +-- AuthProvider
  |     |
  |     +-- Identity API /auth/login
  |     +-- JWT role claims
  |     +-- sessionStorage
  |
  +-- Role-aware router
  |
  +-- Application shell
  |
  +-- Service pages
        |
        +-- Literature Intelligence
        +-- Target Discovery
        +-- Chemical Design
        +-- Compound Catalog
        +-- Screening
        +-- Assay Management
        +-- AI Discovery
        +-- Workflow
        +-- Notification
        +-- Search
        +-- Reporting
        +-- Audit
        +-- Identity
```

## Security

The frontend does not define the authorization boundary.

The backend APIs must continue to validate JWT issuer, audience, signing key,
expiration and roles through `Algocura.Platform.Security`.

A hidden menu or blocked route does not replace API authorization.

## API URL strategy

All backend origins are build-time Vite environment variables. This keeps the
frontend repository independent from a specific local, test or deployed
endpoint topology.
