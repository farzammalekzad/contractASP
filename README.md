# Contract Management MVP

An ASP.NET Core MVC application for managing contracts, progress-payment statements, and received payments. The primary experience is a server-rendered Persian RTL Razor UI.

## Stack

- .NET 10 (`net10.0`), ASP.NET Core MVC, Razor Views
- EF Core 10.0.12 with SQL Server
- Bootstrap 5.3.3 RTL, Bootstrap Icons, Vazirmatn, and custom CSS
- Swagger in Development only

## Run locally

```powershell
dotnet restore backend/ContractInvoice.Api/ContractInvoice.Api.csproj
dotnet build backend/ContractInvoice.Api/ContractInvoice.Api.csproj
dotnet run --project backend/ContractInvoice.Api/ContractInvoice.Api.csproj
```

The checked-in HTTP launch profile uses `http://localhost:5266`. Configure SQL Server with `ConnectionStrings__DefaultConnection`; the checked-in default targets local SQL Express. Do not commit credentials.

## Modules and behavior

- Contracts, progress payments, and payments each have MVC CRUD screens.
- Progress payments belong to a contract; payments belong to a progress payment.
- All Create and Edit forms post to their matching actions and use anti-forgery validation.
- List pages use Bootstrap confirmation modals for delete actions. Contract and progress-payment delete outcomes are returned to their list pages through TempData success/error alerts.
- A contract that has progress payments, or a progress payment that has payments, is not deleted. MVC catches those expected child-record cases and returns the user to the list with an error message. Payments can be deleted directly.
- Details pages are card-based; Create/Edit pages use compact form layouts. The persistent sidebar has active navigation state.

## Routes

The default MVC route is `{controller=Home}/{action=Index}/{id?}`. Main areas are `/Contracts`, `/ProgressPayments`, and `/Payments`. Attribute-routed endpoints remain at `/api/Contract`, `/api/Test`, and `/api/health`; MVC is the primary UI, and the Contract API does not map all contract fields (see [architecture](docs/ARCHITECTURE.md)).

## Database and migrations

The SQL Server schema uses restricted foreign keys and unique contract/progress-payment identifiers. Inspect [DATABASE.md](docs/DATABASE.md) before working with EF Core migrations. Do not edit applied migrations, reset the database, or alter financial delete behavior without an approved plan.

## Development notes

- Preserve `lang="fa" dir="rtl"`, the MVC/Razor architecture, and the existing UI conventions.
- Dates are persisted as strings. Changing this requires a migration and data-conversion plan.
- There are no automated tests in the repository.
- Known remaining limitations and safe boundaries are in [AI_CONTEXT.md](docs/AI_CONTEXT.md) and [ROADMAP.md](docs/ROADMAP.md).
