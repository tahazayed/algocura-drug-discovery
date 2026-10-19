# Algocura.FrontEnd — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 45.

## Original documented intent

# Algocura.DrugDiscovery.FrontEnd

React frontend for the Algocura Drug Discovery Platform.

## Stack

- React 19.2
- TypeScript
- Vite 8.1
- React Router 8
- Material UI 9
- Native Fetch API
- Vitest
- React Testing Library

## Supported backend services

The application provides navigation and API integration for:

- Literature Intelligence
- Target Discovery
- Chemical Design
- Compound Catalog
- Screening
- Assay Management
- AI Discovery
- Workflow
- Notification
- Search
- Reporting
- Audit
- Identity

## Authorization model

The UI reads role claims from the JWT issued by
`Algocura.Identity.Api`.

Supported roles:

```text
Administrator
Scientist
Reviewer
Service
```

The current frontend route model is:

| Area | Roles |
|---|---|
| Scientific discovery services | Scientist, Reviewer, Administrator |
| Audit | Reviewer, Administrator |
| Identity user administration | Administrator |
| Service synchronization endpoints | Not exposed as normal UI navigation |

Frontend authorization is only a user-experience control. The ASP.NET Core
`[Authorize]` attributes on the APIs remain the security boundary.

## Local development

Requirements:

```text
Node.js >= 22.22.0
npm
```

Copy the environment template:

```powershell
Copy-Item .env.example .env
```

Install packages:

```powershell
npm install
```

Run:

```powershell
npm run dev
```

Open:

```text
http://localhost:5173
```

## Default local service ports

The defaults align with the Algocura local microservice layout:

```text
7001 Literature Intelligence
7002 Target Discovery
7003 Chemical Design
7004 Compound Catalog
7005 Screening
7006 Assay Management
7007 AI Discovery
7008 Workflow
7009 Notification
7010 Search
7011 Reporting
7012 Audit
7013 Identity
```

Every URL is configurable through `VITE_*_API_URL`.

## Login

The frontend calls:

```text
POST /api/v1/auth/login
```

and expects:

```json
{
  "accessToken": "<jwt>"
}
```

The access token is stored in `sessionStorage` and attached to API requests as:

```text
Authorization: Bearer <token>
```

For production, consider moving to a BFF/HttpOnly-cookie architecture if you
want to avoid browser-accessible bearer-token storage.

## Seed users

Your local Identity API can provide test users such as:

```text
admin@algocura.local       Administrator
scientist@algocura.local   Scientist
reviewer@algocura.local    Reviewer
service@algocura.local     Service
```

The system account is passwordless and should not be used for interactive
login.

## CORS

When running the Vite dev server at `http://localhost:5173`, each backend API
must allow that origin.

A typical ASP.NET Core development CORS policy is:

```csharp
builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "FrontendDevelopment",
            policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });
```

and:

```csharp
app.UseCors(
    "FrontendDevelopment");
```

Apply CORS before authentication/authorization middleware.

## Build

```powershell
npm run build
```

Output:

```text
dist/
```

## Docker

Build:

```powershell
docker build -t algocura/algocura-drugdiscovery-frontend:dev .
```

Run:

```powershell
docker run --rm -p 8088:8080 algocura/algocura-drugdiscovery-frontend:dev
```

Or:

```powershell
docker compose -f compose.frontend.yml up --build
```

Open:

```text
http://localhost:8088
```

## Repository structure

```text
Algocura.DrugDiscovery.FrontEnd
│
├── src
│   ├── api
│   ├── auth
│   ├── components
│   ├── config
│   ├── layout
│   ├── pages
│   ├── routes
│   ├── test
│   ├── types
│   ├── App.tsx
│   ├── main.tsx
│   └── theme.ts
│
├── .env.example
├── .gitignore
├── .nvmrc
├── Dockerfile
├── compose.frontend.yml
├── eslint.config.js
├── index.html
├── nginx.conf
├── package.json
├── tsconfig.app.json
├── tsconfig.json
├── tsconfig.node.json
└── vite.config.ts
```

## Current scope

This repository is a complete runnable frontend foundation with:

- Identity login
- JWT parsing
- bearer-token API calls
- authenticated routes
- role-based navigation
- role-based route guards
- dashboard
- service pages for all platform APIs
- search page
- generic JSON data tables
- error/loading states
- responsive Material UI application shell
- environment-driven API URLs
- Docker/Nginx production image
- basic JWT unit test

The generic service pages intentionally tolerate the current heterogeneous
response shapes of the microservices. They can be incrementally replaced by
domain-specific forms, tables and workflows while retaining the shared
authentication, routing and layout foundation.


## Reporting pages

Reporting is split into two frontend pages:

```text
/reporting             Report definitions and CRUD operations
/reporting/analytics   Reporting charts and KPI summaries
```

The definitions table formats Reporting status values as Draft, Active, or
Retired, formats UTC timestamps for display, and abbreviates long IDs while
preserving the full value in the cell tooltip.

The analytics page uses existing Reporting API endpoints:

```text
GET /api/v1/reporting/summary
GET /api/v1/report-runs
```

It displays total projections, active report definitions, completed/total run
counts, projection distribution, report-run status, and the latest available
report metrics. No additional chart package is required.


## Package manifest
```json
{
  "name": "algocura.frontend",
  "private": true,
  "version": "0.1.0",
  "type": "module",
  "engines": {
    "node": ">=22.22.0"
  },
  "scripts": {
    "dev": "vite --host 0.0.0.0",
    "build": "tsc -b && vite build",
    "preview": "vite preview --host 0.0.0.0",
    "lint": "eslint .",
    "test": "vitest run",
    "test:watch": "vitest"
  },
  "dependencies": {
    "@emotion/react": "^11.14.0",
    "@emotion/styled": "^11.14.0",
    "@mui/material": "^9.3.1",
    "react": "^19.2.0",
    "react-dom": "^19.2.0",
    "react-router": "^8.2.0"
  },
  "devDependencies": {
    "@eslint/js": "^9.39.0",
    "@testing-library/jest-dom": "^6.9.0",
    "@testing-library/react": "^16.3.0",
    "@types/node": "^24.10.0",
    "@types/react": "^19.2.0",
    "@types/react-dom": "^19.2.0",
    "@vitejs/plugin-react": "^6.0.5",
    "eslint": "^9.39.0",
    "eslint-plugin-react-hooks": "^7.0.0",
    "eslint-plugin-react-refresh": "^0.4.24",
    "globals": "^16.5.0",
    "jsdom": "^27.2.0",
    "typescript": "^5.9.0",
    "typescript-eslint": "^8.46.0",
    "vite": "^8.1.0",
    "vitest": "^4.0.0"
  }
}

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.FrontEnd/ARCHITECTURE.md` | 49 |
| `Algocura.FrontEnd/README.md` | 307 |
| `Algocura.FrontEnd/index.html` | 16 |
| `Algocura.FrontEnd/package.json` | 43 |
| `Algocura.FrontEnd/src/App.tsx` | 5 |
| `Algocura.FrontEnd/src/api/httpClient.ts` | 73 |
| `Algocura.FrontEnd/src/api/identityApi.ts` | 23 |
| `Algocura.FrontEnd/src/auth/AuthContext.tsx` | 118 |
| `Algocura.FrontEnd/src/auth/RequireAuth.tsx` | 19 |
| `Algocura.FrontEnd/src/auth/RequireRole.tsx` | 20 |
| `Algocura.FrontEnd/src/auth/jwt.test.ts` | 30 |
| `Algocura.FrontEnd/src/auth/jwt.ts` | 58 |
| `Algocura.FrontEnd/src/components/ErrorState.tsx` | 9 |
| `Algocura.FrontEnd/src/components/JsonTable.test.tsx` | 41 |
| `Algocura.FrontEnd/src/components/JsonTable.tsx` | 130 |
| `Algocura.FrontEnd/src/components/LoadingState.tsx` | 20 |
| `Algocura.FrontEnd/src/components/PageHeader.tsx` | 28 |
| `Algocura.FrontEnd/src/components/RecordDialog.test.tsx` | 86 |
| `Algocura.FrontEnd/src/components/RecordDialog.tsx` | 249 |
| `Algocura.FrontEnd/src/components/ReportingCharts.tsx` | 491 |
| `Algocura.FrontEnd/src/components/ReportingNavigation.tsx` | 35 |
| `Algocura.FrontEnd/src/config/services.test.ts` | 29 |
| `Algocura.FrontEnd/src/config/services.ts` | 233 |
| `Algocura.FrontEnd/src/layout/AppShell.tsx` | 240 |
| `Algocura.FrontEnd/src/main.tsx` | 25 |
| `Algocura.FrontEnd/src/pages/ClinicalResearchPage.tsx` | 449 |
| `Algocura.FrontEnd/src/pages/DashboardPage.tsx` | 64 |
| `Algocura.FrontEnd/src/pages/ForbiddenPage.tsx` | 24 |
| `Algocura.FrontEnd/src/pages/GenericServicePage.tsx` | 170 |
| `Algocura.FrontEnd/src/pages/LoginPage.tsx` | 162 |
| `Algocura.FrontEnd/src/pages/NotFoundPage.tsx` | 17 |
| `Algocura.FrontEnd/src/pages/ReportingAnalyticsPage.tsx` | 491 |
| `Algocura.FrontEnd/src/pages/SearchPage.tsx` | 107 |
| `Algocura.FrontEnd/src/routes/AppRoutes.tsx` | 108 |
| `Algocura.FrontEnd/src/test/setup.ts` | 7 |
| `Algocura.FrontEnd/src/theme.ts` | 48 |
| `Algocura.FrontEnd/src/types/auth.ts` | 23 |
| `Algocura.FrontEnd/src/types/common.ts` | 54 |
| `Algocura.FrontEnd/src/vite-env.d.ts` | 21 |
| `Algocura.FrontEnd/tsconfig.app.json` | 21 |
| `Algocura.FrontEnd/tsconfig.json` | 7 |
| `Algocura.FrontEnd/tsconfig.node.json` | 11 |
| `Algocura.FrontEnd/vite-env.d.ts` | 24 |
| `Algocura.FrontEnd/vite.config.d.ts` | 2 |
| `Algocura.FrontEnd/vite.config.ts` | 16 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
