# Domain model

## Contract

`Contract` is the root entity. It has required-by-model `ContractCode`, `Title`, `ContractType`, `ProjectCode`, `Employer`, and `Amount`; optional manager, sponsor, advance amount, letter metadata, date strings, and related revenue-contract code. `ContractCode` is globally unique and limited to 50 characters. No duration is calculated, stored, validated, or displayed.

## Progress payment

`ProgressPayment` is a contract statement with a required relationship to one contract, positive `PaymentNo`, `Title`, `PaymentType`, and non-negative `ProposedAmount`; approved amount and correspondence/period/status fields are optional. Valid types are exactly `TEMPORARY`, `FINAL`, and `ADJUSTMENT`. `(ContractId, PaymentNo)` is unique. A progress payment owns zero or more received payments.

## Payment

`Payment` records a received amount for one progress payment. It requires a parent ID, a payment-date string, and non-negative `ReceivedAmount`; reference number and description are optional. A progress-payment Details page displays total received and, when an approved amount exists, `ApprovedAmount - total paid`. The application permits total payments to exceed the approved or proposed amount.

## Relationships and deletion

```text
Contract 1 --- * ProgressPayment 1 --- * Payment
```

Both relationships are required and restricted. A contract with progress payments and a progress payment with payments cannot be deleted. MVC handles both expected cases gracefully by redirecting to the applicable list and showing a TempData error. A payment has no children and can be deleted. No financial records are cascade-deleted.
