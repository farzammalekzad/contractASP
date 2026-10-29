# AI coding-agent handoff

## Current state to preserve

The MVP is a .NET 10 MVC/Razor application with SQL Server persistence for contracts, progress payments, and received payments. Preserve the Persian RTL layout, shared sidebar, active navigation, card Details views, compact sectioned forms, and Bootstrap modal delete UI. Do not introduce a SPA or change date storage, schema, migration history, unique indexes, decimal precision, or restricted financial deletes without approval.

MVC edit forms post correctly to `Edit`. Contract/progress-payment deletion checks for children in the service and MVC redirects with TempData success/error messages. Do not describe those as open bugs. All Create/Edit views include the shared client-validation partial.

## Current technical debt / limitations

- No global exception handling strategy or centralized error page exists; unexpected persistence/runtime failures can surface as HTTP 500 responses.
- `/api/Contract` POST/PUT map only code, title, and amount despite contract DTOs exposing more fields. API DELETE does not gracefully catch child-record deletion failures as MVC does.
- Payment delete redirects without a TempData success/error alert, unlike contract and progress-payment delete flows.
- Validation is uneven: database length limits for several Contract and Payment fields are not all mirrored in DTO annotations; services do not validate every string constraint before persistence.
- The system does not prevent overpayment against proposed or approved amounts.
- No automated test project or checked-in automated tests exist.
- The historical duplicate ContractCode-index migration issue may break a clean migration chain; do not edit applied migration files.
- Source viewing in the current shell can display Persian literals as mojibake; preserve file encoding and verify rendered text before editing UI literals.

## Working conventions

- Map DTOs to entities in controllers and retain service-level checks plus database integrity constraints.
- MVC controllers are plural; `ContractController` is the singular API controller.
- For expected MVC validation failures, return the same view with model errors and restore select-list data.
- Confirm both MVC and API behavior before claiming an endpoint follows a UI behavior.
