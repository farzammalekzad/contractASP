# Roadmap

## Completed MVP

- Contract, progress-payment, and payment CRUD with EF Core/SQL Server persistence
- MVC/Razor Persian RTL UI with sidebar and active navigation
- Card-based Details pages and compact Create/Edit forms
- Correct Edit POST targets and shared client-side validation rendering
- Bootstrap modal confirmations on all list-page delete actions
- Graceful MVC handling and TempData feedback for contract/progress-payment child-record delete blocks
- Unique identifiers, restricted financial relationships, and core validation

## Remaining hardening and next phases

- Add automated tests for services, MVC validation, delete outcomes, and API/MVC mapping differences.
- Add centralized exception handling and a user-facing error experience.
- Decide whether API contract mapping should be made complete and whether API deletes should match MVC's graceful child-record response.
- Complete/align validation with database field limits and establish an overpayment policy.
- Assess and approve a remediation strategy for the historical duplicate ContractCode-index migration chain.
- Consider authentication/Active Directory, roles, workflow/status management, document management, search/filtering, dashboard/reporting, audit logging, and production deployment preparation.

Future work should retain MVC/Razor and the Persian RTL conventions unless an explicit architecture decision changes them.
