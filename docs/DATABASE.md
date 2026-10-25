# Database

## Schema

`AppDbContext` uses SQL Server and exposes `Contracts`, `ProgressPayments`, and `Payments`, each with an integer identity `Id`.

| Principal | Dependent | Foreign key | Delete behavior |
| --- | --- | --- | --- |
| Contracts | ProgressPayments | `ProgressPayments.ContractId` | Restrict |
| ProgressPayments | Payments | `Payments.ProgressPaymentId` | Restrict |

`Contracts.ContractCode` has a unique index and is `nvarchar(50)`. `ProgressPayments` has a unique `(ContractId, PaymentNo)` index. Amount, advance amount, proposed/approved amount, and received amount use `decimal(18,2)`. Progress-payment/payment dates are `nvarchar(20)` strings; contract dates are nullable strings (`nvarchar(max)` in the migration snapshot). Payment reference and description are limited to 100 and 500 characters respectively.

## Checked-in migration history

| ID | Name | Effect |
| --- | --- | --- |
| 20261008153404 | `initialCreate` | Creates the original Contracts table. |
| 20261009082905 | `AddUniqueContractCode` | Creates the ContractCode unique index. |
| 20261009164706 | `FixContractCodeSchema` | Empty migration. |
| 20261009170625 | `CorrectContractCodeSchema` | Alters ContractCode to `nvarchar(50)` and creates the same-named unique index. |
| 20261012175756 | `ExpandContractShema` | Adds current contract detail fields. |
| 20261012183459 | `AddProgressPayments` | Creates progress payments, their composite unique index, and restricted Contract FK. |
| 20261015181211 | `AddPayments` | Creates payments and the restricted ProgressPayment FK. |

### Historical migration risk

`AddUniqueContractCode` and `CorrectContractCodeSchema` both create `IX_Contracts_ContractCode`. A fresh sequential application appears likely to conflict. This is checked-in history, not permission to rewrite it: inspect the target database's `__EFMigrationsHistory` and make an approved remediation plan before any migration work.

## Safety rules

- Treat applied migrations as immutable.
- Create and review a new migration for every approved schema change; do not drop/recreate the database or reset data without explicit authorization.
- Preserve unique indexes, `decimal(18,2)`, string-date storage, and restricted financial foreign keys unless the approved work includes a data and migration strategy.
