# Architecture

## Application shape

This is one .NET 10 ASP.NET Core MVC project. Razor Views are the implemented frontend; there is no SPA, React, TypeScript, Blazor, or separate frontend project. `Program.cs` registers controllers with views, EF Core SQL Server, Swagger, and scoped services. Swagger is enabled only in Development. Static files, the conventional MVC route, and attribute-routed controllers are mapped. There is no global exception-handling middleware or centralized error view.

```text
Browser -> MVC controller -> DTO/model validation -> service -> AppDbContext -> SQL Server
    ^                                                                    |
    +---------------- Razor view / redirect / model errors -------------+
```

MVC controllers are `HomeController`, `ContractsController`, `ProgressPaymentsController`, and `PaymentsController`. Posts use anti-forgery validation. `ContractController` and `TestController` are secondary attribute-routed APIs; `/api/health` is a minimal endpoint.

## Services, data, and validation

`ContractService`, `IProgressPaymentService`/`ProgressPaymentService`, and `IPaymentService`/`PaymentService` are scoped. Controllers map DTOs to entities; services enforce relationship existence, uniqueness, allowed progress-payment types, and non-negative amounts. `AppDbContext` owns `Contracts`, `ProgressPayments`, and `Payments` and configures indexes, column limits/precision, and `Restrict` foreign keys.

DTO annotations provide MVC/API model validation. Contract DTOs require core identity fields and bound the code length; progress-payment DTOs additionally cover positive IDs/numbers, type pattern, relevant lengths, and monetary ranges. Payment DTOs cover parent, date presence, and non-negative amount. Several entity/database length constraints are not mirrored by Contract/Payment DTO annotations, so database/service failures remain possible for oversized optional inputs.

## MVC versus API mapping

MVC contract create/edit explicitly maps all current contract fields. Progress-payment and payment MVC paths map their full DTOs. Edit views post to `Edit`, not `Create`.

The singular `/api/Contract` controller exposes GET, GET by ID, POST, PUT, and DELETE, but POST/PUT map only `ContractCode`, `Title`, and `Amount`; the remaining fields accepted by the contract DTOs are ignored. Its DELETE also does not translate the service's child-progress-payment `InvalidOperationException`, unlike the MVC flow. Treat API and MVC behavior as intentionally different current behavior until a separately approved API change.

## UI conventions and delete flow

The shared layout sets Persian RTL (`lang="fa" dir="rtl"`), loads Bootstrap RTL/Vazirmatn/icons/custom CSS, and renders the persistent sidebar. Sidebar links use the current controller to indicate the active area. List views use tables and Bootstrap modal confirmations; Details views use cards; Create/Edit views use compact, sectioned forms. All six Create/Edit views render the shared jQuery unobtrusive-validation partial.

Contract and progress-payment list pages render TempData `SuccessMessage` and `ErrorMessage` alerts. Their services pre-check children before delete, and MVC catches the resulting `InvalidOperationException`, redirects to Index, and reports the outcome. Payment deletion redirects directly without a TempData confirmation alert. Legacy GET `Delete/{id}` views still exist, but list-page modal POSTs are the normal UI path.
