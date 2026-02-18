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
