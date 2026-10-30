# Policy Tracker Web API

Policy Tracker Web API is a RESTful ASP.NET Core Web API powering the Policy Tracker platform. It provides secure endpoints for managing clients, insurance and LIC policies, renewals, premiums, broker statements, reconciliation, and commission tracking. The API uses JWT authentication, role-based access control, and structured logging, and is built on a clean layered architecture with EF Core. It serves the Policy Tracker Web Console.

## Features

- **Authentication** — JWT-based sign-in, token issuance, and secure API access.
- **Role-based access control** — distinct roles for Admins, Brokers/Agents, and Clients.
- **Client management** — manage client records, contact details, and KYC documents.
- **Policy management** — record and manage Life Insurance (LIC), General Insurance, Health, and Motor policies.
- **Renewal tracking** — track upcoming renewals, send reminders, and record renewal status.
- **Premium tracking** — record premium amounts, due dates, payment status, and receipts.
- **Broker statements** — generate and manage statements for brokers/agents.
- **Reconciliation** — reconcile premium payments, commissions, and policy statuses.
- **Commission tracking** — calculate and track commissions for brokers/agents based on policy types.
- **Logging and auditing** — structured request/response logging and audit trails for financial data.
- **API documentation** — Swagger/OpenAPI for exploring and testing endpoints.

> Feature descriptions are based on the controllers, domain models, services, and repositories present in this repository. Actual behavior may depend on the connected SQL database and configuration.

## Technology stack

- C# and ASP.NET Core `<6 / 7 / 8>`
- Clean layered architecture (API, Application, Domain, Infrastructure)
- Entity Framework Core (Code First / Database First — `<pick one>`)
- SQL Server database
- JWT authentication with role-based authorization
- Structured logging via `<Serilog / built-in ILogger>`
- Swagger / OpenAPI for API documentation
- Visual Studio solution (`PolicyTracker.API.sln`)

## Solution structure

```text
PolicyTracker/
├── PolicyTracker.API.sln
├── PolicyTracker.API/                    # ASP.NET Core Web API host
│   ├── Controllers/                      # API endpoints
│   ├── Middleware/                       # Exception handling, logging, auth
│   ├── Filters/                          # Action filters and validation
│   ├── Extensions/                       # Service registration extensions
│   ├── Config/                           # Environment-specific app settings
│   └── Program.cs
├── PolicyTracker.Application/            # Use cases, DTOs, interfaces, validators
├── PolicyTracker.Domain/                 # Entities, value objects, enums
└── PolicyTracker.Infrastructure/         # EF Core DbContext, migrations, repositories