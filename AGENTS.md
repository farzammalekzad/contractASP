# Repository development instructions

## Project and documentation

This repository contains a contract-management application for principals, contracts, contract attachments, progress-payment statements, and payment documents. The primary interface is Persian RTL ASP.NET Core MVC with Razor views.

Read these documents according to the task:

- [SYSTEM_ARCHITECTURE.md](SYSTEM_ARCHITECTURE.md): verified application structure and dependencies.
- [DEVELOPMENT_STATUS.md](DEVELOPMENT_STATUS.md): dated implementation status, evidence, limitations, and verification backlog.
- [PAYMENT_MODULE_ANALYSIS.md](PAYMENT_MODULE_ANALYSIS.md): **detailed authoritative specification for payment module design and approved business rules**. Its original baseline observations and proposed-phase wording are historical; use DEVELOPMENT_STATUS for implementation/deployment status. Do not reinterpret historical defects as approved behavior.

Older README and `docs/` documents contain stale descriptions; consult the discrepancy register in DEVELOPMENT_STATUS. Source establishes what currently exists; approved requirements establish intended behavior. Report conflicts rather than silently changing requirements. Keep detailed module specifications out of this file.

## Verified stack and structure

- Single `backend/ContractInvoice.Api/ContractInvoice.Api.csproj`: .NET 10, ASP.NET Core MVC/Razor, nullable reference types and implicit usings enabled.
- EF Core SQL Server and Design 10.0.12; Swashbuckle 10.1.0. Bootstrap 5 RTL, Bootstrap Icons, Vazirmatn, Jalali datepicker, custom CSS, and plain JavaScript.
- `Controllers/`: MVC actions and secondary attribute-routed APIs; `DTOs/`: input contracts; `ViewModels/`: presentation contracts; `Model/`: entities; `Services/` and `Interfaces/`: application behavior/contracts; `Data/`: EF configuration; `Migrations/`: migration history/snapshot; `Views/`: Razor; `wwwroot/`: static assets; `Program.cs`: composition and routing.
- No SPA or separate frontend build is established. The root npm manifest does not define an application build/test pipeline.

## Architecture and coding conventions

- Preserve MVC -> service -> EF Core -> SQL Server as the main request flow. Controllers bind/map inputs and select responses; services own business validation and financial calculations. Some existing controllers access EF directly (attachments); inspect actual dependencies before changing them.
- Use explicit input mapping, established DTOs/display models, scoped DI, existing naming, and UTF-8 Persian text. Do not introduce a repository layer, framework migration, or broad namespace cleanup as incidental work.
- MVC form POSTs use anti-forgery validation. Retain server-side validation even with client validation; restore entered values and selection data when returning errors.
- Payment actions use `IPaymentDocumentService` and `PaymentServiceResult<T>`. Preserve review tokens, explicit confirmation, stale RowVersion rejection, and transactional aggregate writes. Do not reactivate legacy `_context.Payments` paths.
- Translate expected failures into recoverable messages. Other services currently use expected exceptions and MVC ModelState/TempData. Do not expose raw SQL errors or silently treat every persistence error as a duplicate.

## Database, money, and dates

- `AppDbContext` owns EF relationships, column mappings, precision, indexes, and constraints. `ProgressPayment` maps to `invoice`; preserve its explicit legacy column mappings.
- Preserve required relationships and restricted financial deletes. Attachment metadata currently has a cascade FK; do not assume this also deletes physical files.
- Use C# `decimal` and established `decimal(18,2)` mappings for monetary values. Payment amounts are IRR (Rial), never implicitly Toman. Enforce two-decimal precision without silent rounding; keep the six allocation financial fields independent of allocation totals.
- Server calculations are authoritative. Transfer monetary values as decimal strings where required by the payment frontend; do not use JavaScript floating-point arithmetic for financial authority.
- Preserve Persian `lang="fa" dir="rtl"`, Bootstrap 5 RTL, two-decimal payment displays, and Jalali date strings. Reuse the existing datepicker; do not convert business dates to Gregorian storage. Technical timestamps such as attachment upload time may use UTC DateTime.

## Security and diagnostics

- No application authentication, Identity, role/policy authorization, or authentication/authorization middleware is currently configured. Anti-forgery and Data Protection are not access control. Do not claim endpoints are protected or invent identity conventions; authentication changes require an explicit design/task.
- Never commit or log credentials, connection strings, review tokens, or unnecessary sensitive data. Preserve upload size/extension validation, generated stored filenames, and path containment checks. File extension checks do not establish malware safety.
- Use `ILogger` for useful server diagnostics and safe user-facing messages. Payment persistence failures currently log warnings. Swagger is Development-only; `/api/health` is a liveness response, not a database readiness probe. Global exception handling and a comprehensive audit trail are not established.

## Git, database safety, and mandatory workflow

1. Inspect the working tree, existing source, and relevant specialized documentation before modifying a module.
2. Treat approved business rules as authoritative. Never change them without explicit user approval. Avoid unrelated refactoring and preserve unrelated working-tree changes.
3. Never execute migrations without explicit approval. Never perform destructive database operations without explicit approval. Verify the approved target environment before any authorized database work; never assume Development permission extends to another environment.
4. Treat applied migrations as immutable. For approved schema changes, prepare a targeted forward migration, inspect its migration/designer/snapshot changes, and review data impact. Migration generation and application are separate steps; generating a migration does not authorize applying it.
5. Follow a user-specified branch workflow. No mandatory branch naming convention is established; use a descriptive task branch when requested. Keep any authorized commit focused. Never commit or push without explicit approval.
6. Never use destructive Git commands such as `reset` or `restore` without explicit approval, including for build artifacts. Inspect diffs before proposing cleanup; do not discard someone else's work.
7. Run appropriate builds and tests after code changes. Standard build: `dotnet build backend/ContractInvoice.Api/ContractInvoice.Api.csproj` (`--no-restore` when dependencies are already restored). Do not start database-backed verification without the required authorization.
8. Financial/concurrency changes need meaningful SQL Server integration coverage; an in-memory provider cannot prove locking, rowversion, or SQL constraints. No automated test suite is currently established; distinguish this gap from a passing build or manual smoke test.
9. Report actual commands/results, warnings, created/modified files, and known limitations. Never claim unexecuted tests passed. Keep implementation/deployment evidence updated in DEVELOPMENT_STATUS and update architecture documentation when architecture changes.
