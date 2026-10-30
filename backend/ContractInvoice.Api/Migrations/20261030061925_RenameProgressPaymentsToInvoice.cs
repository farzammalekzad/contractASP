using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractInvoice.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameProgressPaymentsToInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_ProgressPayments_ProgressPaymentId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgressPayments_Contracts_ContractId",
                table: "ProgressPayments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProgressPayments",
                table: "ProgressPayments");

            migrationBuilder.RenameTable(
                name: "ProgressPayments",
                newName: "invoice");

            migrationBuilder.RenameColumn(
                name: "RequestLetterNo",
                table: "invoice",
                newName: "OceRequestLetterNo");

            migrationBuilder.RenameColumn(
                name: "RequestLetterDate",
                table: "invoice",
                newName: "OceRequestLetterDate");

            migrationBuilder.RenameColumn(
                name: "ProposedAmount",
                table: "invoice",
                newName: "RequestPrice");

            migrationBuilder.RenameColumn(
                name: "PeriodStartDate",
                table: "invoice",
                newName: "InvoiceStartDte");

            migrationBuilder.RenameColumn(
                name: "PeriodEndDate",
                table: "invoice",
                newName: "InvoiceEndDte");

            migrationBuilder.RenameColumn(
                name: "PaymentType",
                table: "invoice",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "PaymentNo",
                table: "invoice",
                newName: "No");

            migrationBuilder.RenameColumn(
                name: "ApprovedAmount",
                table: "invoice",
                newName: "ApprovedPrice");

            migrationBuilder.RenameColumn(
                name: "ApprovalLetterNo",
                table: "invoice",
                newName: "PrincipalApprovedLetterNo");

            migrationBuilder.RenameColumn(
                name: "ApprovalDate",
                table: "invoice",
                newName: "PrincipalApprovedLetterDate");

            migrationBuilder.RenameIndex(
                name: "IX_ProgressPayments_ContractId_PaymentNo",
                table: "invoice",
                newName: "IX_invoice_ContractId_No");

            migrationBuilder.AddColumn<string>(
                name: "Header_ContractNo",
                table: "invoice",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_invoice",
                table: "invoice",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_Contracts_ContractId",
                table: "invoice",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_invoice_ProgressPaymentId",
                table: "Payments",
                column: "ProgressPaymentId",
                principalTable: "invoice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_Contracts_ContractId",
                table: "invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_invoice_ProgressPaymentId",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_invoice",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "Header_ContractNo",
                table: "invoice");

            migrationBuilder.RenameTable(
                name: "invoice",
                newName: "ProgressPayments");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "ProgressPayments",
                newName: "PaymentType");

            migrationBuilder.RenameColumn(
                name: "RequestPrice",
                table: "ProgressPayments",
                newName: "ProposedAmount");

            migrationBuilder.RenameColumn(
                name: "PrincipalApprovedLetterNo",
                table: "ProgressPayments",
                newName: "ApprovalLetterNo");

            migrationBuilder.RenameColumn(
                name: "PrincipalApprovedLetterDate",
                table: "ProgressPayments",
                newName: "ApprovalDate");

            migrationBuilder.RenameColumn(
                name: "OceRequestLetterNo",
                table: "ProgressPayments",
                newName: "RequestLetterNo");

            migrationBuilder.RenameColumn(
                name: "OceRequestLetterDate",
                table: "ProgressPayments",
                newName: "RequestLetterDate");

            migrationBuilder.RenameColumn(
                name: "No",
                table: "ProgressPayments",
                newName: "PaymentNo");

            migrationBuilder.RenameColumn(
                name: "InvoiceStartDte",
                table: "ProgressPayments",
                newName: "PeriodStartDate");

            migrationBuilder.RenameColumn(
                name: "InvoiceEndDte",
                table: "ProgressPayments",
                newName: "PeriodEndDate");

            migrationBuilder.RenameColumn(
                name: "ApprovedPrice",
                table: "ProgressPayments",
                newName: "ApprovedAmount");

            migrationBuilder.RenameIndex(
                name: "IX_invoice_ContractId_No",
                table: "ProgressPayments",
                newName: "IX_ProgressPayments_ContractId_PaymentNo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProgressPayments",
                table: "ProgressPayments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_ProgressPayments_ProgressPaymentId",
                table: "Payments",
                column: "ProgressPaymentId",
                principalTable: "ProgressPayments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressPayments_Contracts_ContractId",
                table: "ProgressPayments",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
