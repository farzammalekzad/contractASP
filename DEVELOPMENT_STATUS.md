# Development status

Last reviewed: 2026-10-31. Scope: repository source inspection plus explicitly attributed reports from this conversation. This documentation update did not run a build, test, application, database query, or migration.

## Evidence and verification ledger

| Evidence | Status and limits |
| --- | --- |
| Current source inspection | MVC/Razor implementation, services, EF mappings, checked-in migration files, and static frontend assets inspected. This establishes source presence, not exhaustive runtime correctness. |
| Latest Codex-reported build | `dotnet build backend/ContractInvoice.Api/ContractInvoice.Api.csproj --no-restore` succeeded with **0 errors and 8 warnings** during Phase 4B. Not rerun during this documentation task. |
| Other Codex checks in Phase 4B | `node --check backend/ContractInvoice.Api/wwwroot/js/payment-document.js` and `git diff --check` passed. These are syntax/whitespace checks, not integration tests. |
| Development deployment | **User reports** that migration `20261030175050_ReplacePaymentsWithPaymentDocumentsAndAllocations` was applied successfully to the Development database. The legacy Payments table was replaced by PaymentDocuments and PaymentAllocations. No independent database/history query was performed here. |
| Manual application testing | **User reports** successful manual testing in Development, that the application runs successfully, and that the overall system works well. No detailed scenario matrix was supplied; do not infer that every edge case was exercised. |
| Automated tests | No automated test project, test suite, or test pipeline found in the repository inspection. No automated SQL Server integration test execution is evidenced. `TestController` returns a greeting and is not a test suite. |
| Concurrency testing | Source mechanisms exist, but no evidence of concurrent allocation or multi-user race testing was supplied. Not verified. |

The eight reported build warnings were three dependency advisory warnings (Microsoft.OpenApi 2.3.0: high; System.Security.Cryptography.Pkcs 9.0.11: moderate and high) and five CS8601 nullable-assignment warnings in ContractsController (lines 106, 242, 253 at that build) and ContractController (126, 141). These are the build's reported findings, not a new dependency audit.

## Payment module

The PaymentDocument/PaymentAllocation redesign is implemented in source, and its Development deployment and successful overall manual operation are user-reported. Detailed approved rules remain in [PAYMENT_MODULE_ANALYSIS.md](PAYMENT_MODULE_ANALYSIS.md).

- PaymentDocument headers and PaymentAllocation details replace the active legacy Payment model. One document can allocate across progress payments from multiple contracts.
- EF model and replacement migration implement precision, unique references/pairs, financial checks, restricted FKs, and header RowVersion.
- Backend document services, result/error contracts, persistence helpers, DI, DTOs, MVC actions, and display models are implemented.
- Razor list/create/edit/details/delete views and plain JavaScript support contract selection, multiple allocation rows, six independent financial fields, server totals, previews, confirmation, and validation feedback.
- Full and Partial workflows are implemented. Saved Full amounts are preserved unless explicitly reselected; protected previews and renewed confirmation support new/reselected Full amounts.
- Source implements ReadCommitted transactions, parameterized UPDLOCK/HOLDLOCK reads, sorted invoice locking, header-first document edit/delete locking, fresh balance validation, and RowVersion conflict handling. These are source findings, not proof of multi-user correctness.
- ProgressPayment mutation/deletion protects allocated statements: identity changes and invalid approval reductions/clearing are rejected; allocated statements cannot be deleted. Contract deletion remains blocked when progress payments exist.
- ProgressPayment details use allocation history, document links, and the enabled preselected New Payment action.
- Old PaymentService/IPaymentService, entity, DTOs, DbSet, and navigation declarations remain as legacy source. The service is unregistered; controllers use the new document service, and EF ignores Payment and the old navigation. Do not reactivate them. Legacy payment models are no longer used in the payment Razor workflow.

## Other modules: source-backed inventory

| Module | Verified source implementation | Limits of the status claim |
| --- | --- | --- |
| Contracts | MVC list/details/create/edit/delete, DTO mapping, ContractService validation, immutable contract number, principal selection, related-contract validation, attachment display, and child-progress-payment delete protection. | Presence does not establish exhaustive business or regression coverage. |
| ProgressPayments | MVC CRUD, contract relationship, Persian payment types including advances, advance number normalization, approval/financial fields, and allocation protections. | No automated regression coverage found. |
| Principals | List/create/edit MVC actions and service validation; principals have active state and contract relationships. | No principal delete action found; do not describe this as complete CRUD. |
| Contract attachments | Upload/download actions, SQL metadata, filesystem storage, configured size limit, extension allowlist, generated filenames, path containment, and attempted file cleanup on failed upload. | No dedicated attachment delete action found. Filesystem/DB consistency and contract-delete file cleanup require verification; FK cascade covers metadata only. |
| Secondary Contract API | GET/POST/PUT/DELETE actions, expanded field mapping, expected validation responses, and delete Conflict handling. | API mapping omits PrincipalId while ContractService requires a valid active principal. Verify/remediate under a separate approved task; MVC mapping includes PrincipalId. |
| Home and diagnostics | Home MVC controller, Development Swagger, `/api/health` liveness response, and `/api/Test` greeting. | No verified operational monitoring system or database readiness check. |
| Authentication/authorization | No application auth registration, middleware, or authorization attributes found. | No identity/role-based protection should be assumed. |

## Checked-in migration chain

This is the source-file sequence, not an independently queried database history:

1. `20261025191908_InitialContractInvoiceSchema`
2. `20261027185901_AddPrincipalsAndContractRelationship`
3. `20261029083324_AddContractAttachments`
4. `20261030061925_RenameProgressPaymentsToInvoice`
5. `20261030121948_AddPaymentFinancialFields`
6. `20261030175050_ReplacePaymentsWithPaymentDocumentsAndAllocations`

The final migration drops legacy Payments and creates the document/allocation schema; its Down method recreates the legacy structure, not discarded data. The model snapshot reflects the new schema. Application of the final migration in Development is user-reported as above. Other environments, exact history rows, and clean-database migration execution have not been independently verified.

## Remaining verification and hardening

These are pending work items, not changes authorized by this document:

- Establish automated service/MVC tests and real SQL Server integration tests for payment invariants, precision, indexes/FKs, rollback, and migration behavior.
- Run concurrent Full/Partial allocation creation; competing document edits/deletes; approval changes racing with allocations; stale RowVersion; expired/tampered/replayed previews; and deadlock/timeout scenarios. Confirm no over-allocation or partial writes.
- Record a reproducible manual acceptance matrix for validation recovery, Full preservation/reselection, cross-contract selection, deletion, and balances. The reported successful manual test does not replace this coverage.
- Investigate the eight reported build warnings and the Contract API PrincipalId mapping gap.
- Define authentication/authorization and deployment controls before claiming production readiness. Verify Data Protection key persistence/sharing if deploying multiple instances or restarting with outstanding review tokens.
- Assess global exception handling, structured diagnostics, audit requirements, upload hardening, and filesystem cleanup. Do not silently add new audit/voiding policies to the approved payment deletion rules.
- Verify SQL Server isolation settings, deployment configuration, storage permissions, and fresh-install migration behavior in an explicitly authorized environment.
- Reconcile older documentation in a future documentation pass; retain approved payment business rules.

## Documentation discrepancy register

| Existing document | Stale or conflicting statements |
| --- | --- |
| README.md | Describes one legacy payment per statement, old payment deletion behavior, and points to older docs as current limitations. |
| docs/ARCHITECTURE.md | Lists active IPaymentService/Payments persistence; describes old payment DTO/UI behavior. Says Contract API maps only three fields and does not catch delete failures; current API maps many fields and catches InvalidOperationException, although PrincipalId is missing. |
| docs/AI_CONTEXT.md | Says overpayment is unrestricted and carries old API and migration-chain limitations. These are not current payment requirements/status. |
| docs/DATABASE.md | Lists an older October 8–15 migration chain and ContractCode schema; current source has the October 25–30 chain, ContractNo, invoice mapping, principals, attachments, and document allocations. Its duplicate-index warning must not be asserted as a verified defect in the current chain. |
| docs/DOMAIN.md | Describes obsolete ContractCode/Title/Amount names, English progress-payment enum-like strings, and the legacy Payment entity with unrestricted overpayment. Current service types are موقت، قطعی، تعدیل، پیش پرداخت. |
| docs/ROADMAP.md | Treats payment overpayment policy and obsolete migration remediation as open; payment rules are approved and the redesign is implemented. |
| PAYMENT_MODULE_ANALYSIS.md | Original baseline observations and future implementation wording predate implementation. Preserve its approved business rules as authoritative; use this status document for current implementation/deployment evidence. |

No existing document was moved or renamed in this update. Root AGENTS establishes permanent working instructions; SYSTEM_ARCHITECTURE describes current architecture; this document owns dated progress and evidence.
