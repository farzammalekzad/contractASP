# System architecture

Source review date: 2026-10-31. This document describes verified repository architecture, not an independently inspected deployment. See [DEVELOPMENT_STATUS.md](DEVELOPMENT_STATUS.md) for deployment/test evidence and outstanding work, [AGENTS.md](AGENTS.md) for development instructions, and [PAYMENT_MODULE_ANALYSIS.md](PAYMENT_MODULE_ANALYSIS.md) for authoritative payment design and approved business rules.

## Application and solution structure

The application is a single .NET 10 ASP.NET Core Web project at `backend/ContractInvoice.Api/ContractInvoice.Api.csproj`. No separate solution file, domain/data assemblies, SPA, or frontend application project was found. Despite its `Api` name, its primary experience is server-rendered MVC/Razor.

| Location | Responsibility |
| --- | --- |
| `Program.cs` | DI registration, SQL Server provider, Data Protection, MVC/API routing, static files, Development Swagger, health endpoint. |
| `Controllers/` | HTTP request binding, DTO/entity mapping, validation responses, Razor view selection, redirects, and JSON support endpoints. |
| `DTOs/` | Form/API input contracts and annotations; payment inputs carry allocations, review state, and concurrency values. |
| `ViewModels/` | Payment document form/list/details/delete and progress-payment history presentation contracts. |
| `Model/` | EF entities and AllocationType enum; also retained legacy Payment source. |
| `Services/`, `Interfaces/` | Business validation and persistence orchestration; payment service contracts use the Services namespace despite the Interfaces folder. |
| `Data/AppDbContext.cs` | DbSets and EF mappings, indexes, precision, checks, and deletion behavior. |
| `Migrations/` | Forward migrations, generated designers, and current model snapshot. |
| `Views/` | Razor pages and shared partials/layout. |
| `wwwroot/` | Bootstrap RTL, icons, font, Jalali datepicker, custom CSS, payment JavaScript. |
| `Properties/`, application settings | Launch/configuration inputs; deployment values are environment-specific and are not reproduced here. |
| root documents and `docs/` | Development instructions and specifications; older docs have known discrepancies listed in DEVELOPMENT_STATUS. |

The project references EF Core SQL Server/Design 10.0.12 and Swashbuckle SwaggerGen/SwaggerUI 10.1.0. Nullable reference types and implicit usings are enabled. Root `package.json` contains a `git` package dependency but no established frontend build/test scripts.

## Layers, dependencies, and request flow

```text
Browser
  -> MVC route/controller
  -> input binding and ModelState / explicit DTO mapping
  -> application service (validation and business behavior)
  -> AppDbContext / EF Core SQL Server provider
  -> SQL Server
  <- entity/result -> display model -> Razor view, redirect, or JSON response
```

These are logical layers within one assembly, not enforced clean-architecture boundaries. Services reference entities and EF; some payment query methods return presentation records. ContractAttachmentsController directly accesses AppDbContext for metadata and uses ContractAttachmentStorageService for file operations. Other controllers primarily use application services.

`Program.cs` registers scoped ContractService, PrincipalService, ContractAttachmentStorageService, IProgressPaymentService/ProgressPaymentService, and IPaymentDocumentService/PaymentDocumentService. It registers AppDbContext using `ConnectionStrings:DefaultConnection` and adds ASP.NET Core Data Protection. Legacy IPaymentService/PaymentService is not registered.

Main routes use `{controller=Home}/{action=Index}/{id?}`. MVC controllers cover Home, Contracts, Principals, ProgressPayments, Payments, and ContractAttachments. Attribute-routed `/api/Contract` remains a secondary contract API. `/api/Test` returns a greeting; `/api/health` returns a static liveness response. Swagger is enabled only when the hosting environment is Development. Startup contains no automatic migration application.

## Domain and persistence

| Principal | Dependent | Relationship and database behavior |
| --- | --- | --- |
| Principal | Contract | Required PrincipalId FK; restricted deletion. Contract also carries denormalized PrincipalName. |
| Contract | ProgressPayment | Required ContractId FK; restricted deletion. |
| Contract | ContractAttachment | Required ContractId FK; cascade deletion of metadata. Physical files are separate resources. |
| PaymentDocument | PaymentAllocation | Required PaymentDocumentId FK; restricted deletion; service explicitly deletes details and header together. |
| ProgressPayment | PaymentAllocation | Required ProgressPaymentId FK; restricted deletion. |

PaymentDocument and ProgressPayment are connected through allocations. A document can span contracts; allocations derive contract identity from their progress payment rather than duplicating it. Contract related-number fields are service-validated references, not an additional configured self-referencing EF foreign key.

ProgressPayment maps to SQL table `invoice` with explicit legacy column names, including `No`, `Type`, `RequestPrice`, and `ApprovedPrice`. It retains a denormalized Header_ContractNo, while the actual Contract relationship establishes ownership. ContractNo and document PaymentReferenceNo have unique indexes; progress payments have a unique `(ContractId, PaymentNo)` index; allocations have a unique `(PaymentDocumentId, ProgressPaymentId)` index.

The active model includes principals, contracts, attachments, progress payments, documents, and allocations. A legacy `Payments` DbSet declaration and `ProgressPayment.Payments` navigation remain in source, but `Ignore<Payment>()` and ignored navigation configuration exclude them from the EF model. Their presence is not evidence of an active legacy table/workflow.

Financial columns use decimal precision as configured in AppDbContext, including payment `decimal(18,2)`. Payment checks constrain positive document/allocation amounts, valid allocation modes, and nonnegative independent fields. Cross-row balance rules remain service responsibilities. Business dates remain strings using the Jalali conventions; attachment UploadedAt is a UTC technical timestamp.

## Payment persistence and concurrency architecture

PaymentDocumentService owns aggregate writes and returns PaymentServiceResult values with error codes/messages and refreshed previews. It creates a fresh AppDbContext per execution from injected options. PaymentPersistence centralizes SQL Server locking, monetary validation, grouped allocation totals, and expected persistence failure classification.

Source uses explicit ReadCommitted transactions and parameterized `UPDLOCK, HOLDLOCK` reads. Document edits/deletes lock the header before sequential ascending invoice-ID locks; edits include old and proposed targets. ProgressPayment mutations lock the invoice without acquiring document locks afterward. Validation reads follow locking. Header rowversion is checked for stale operations and advanced for detail-only edits.

Full preview confirmation uses a purpose-specific Data Protector, expiry, progress-payment identity, amount, and edit document/version binding. MVC passes tokens and confirmation separately from amounts. All saved financial decisions remain server-side. The implementation and its limits are recorded in DEVELOPMENT_STATUS; lock correctness under actual competing sessions remains a verification task. Do not duplicate or amend detailed payment business rules here.

## Frontend architecture

The shared Razor layout uses `lang="fa" dir="rtl"`, Bootstrap 5 RTL, Bootstrap Icons, Vazirmatn/custom CSS, sidebar navigation, and the Jalali datepicker (`data-jdp`, Latin digit output configured). There is no React/TypeScript/SPA runtime. Existing modules use cards, responsive tables, forms, validation messages, and some modal deletion flows; payment deletion now has a full document review page.

Payment views use the PaymentDocument display/form models and shared `_Form`, `_Allocation`, and `_Document` partials. Inputs bind under `Input.*`; edit/delete forms carry RowVersion. `payment-document.js` manages dynamic allocation rows, contract filtering, duplicate selection prevention, explicit preview requests, confirmation state, and advisory totals.

Eligible-selection, preview, and totals actions return UI support JSON. Monetary response values are formatted as decimal strings, and totals are calculated by the service, not JavaScript. Expected failures preserve form input; stale edits/deletes require an explicit reload/review rather than silently substituting a newer version. ProgressPayment details display allocation history and link to document IDs and preselected document creation.

## Authentication, authorization, validation, and diagnostics

No authentication scheme, ASP.NET Core Identity registration, user/role/policy architecture, authentication/authorization middleware, or authorization attributes were found in the inspected application. This is a current gap, not an implicit public-access policy or evidence of a protected deployment. Hosting/network protections were not inspected.

MVC writes use anti-forgery tokens. Data Protection protects payment review payloads; neither mechanism authenticates users. DTO annotations, ModelState, service validations, and SQL constraints provide complementary validation. Payment services translate expected persistence failures into structured outcomes; other services use exceptions handled by controllers. A global exception handler/centralized error page is not configured in Program.cs.

PaymentDocumentService uses ILogger warnings for mapped persistence failures. A comprehensive audit log or production observability pipeline is not established in source. Framework logging destinations, retention, external monitoring, and deployment-specific log access remain unverified.

Attachment upload metadata is stored in SQL Server; file bodies use the configured filesystem path. The storage service checks size and extension, creates generated names under contract folders, and validates path containment when resolving stored files. Download streams files as application/octet-stream. The upload controller attempts file cleanup after failed persistence; this is not a distributed transaction between filesystem and database. There is no verified malware scanning or authenticated download authorization.

## Migration and deployment strategy

Migration history and the snapshot live alongside the application. The currently checked-in chain starts at `20261025191908_InitialContractInvoiceSchema` and ends at `20261030175050_ReplacePaymentsWithPaymentDocumentsAndAllocations`; the complete sequence and evidence are in DEVELOPMENT_STATUS.

Use reviewed, targeted forward migrations for approved schema changes; preserve unrelated tables and applied migration files. Separate migration generation/review from application. Database changes and destructive operations require explicit authorization under AGENTS. The payment replacement discards legacy payment data by dropping Payments; a Down migration restores structure, not lost records. It must not be treated as a general database reset recipe.

The user reports successful application of the payment migration in Development and successful manual application operation. No database inspection was performed for this documentation. Production topology, environment-specific migration histories, SQL isolation configuration, backups, CI/CD, TLS/reverse proxy configuration, attachment volume durability, and Data Protection key persistence/sharing are not verified.

## Future work versus current architecture

Automated SQL Server integration/concurrency tests, authentication/authorization, centralized exception handling, stronger deployment/observability controls, and reconciliation of older documentation are pending verification/design work. They are not implemented architecture and are not authorized merely by being listed here. Consult DEVELOPMENT_STATUS for the evidence-backed backlog.
