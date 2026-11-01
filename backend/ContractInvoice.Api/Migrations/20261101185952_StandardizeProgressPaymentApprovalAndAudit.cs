using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractInvoice.Api.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeProgressPaymentApprovalAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "invoice",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByPersonnelCode",
                table: "invoice",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "invoice",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByPersonnelCode",
                table: "invoice",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
            
           migrationBuilder.Sql(@"
                UPDATE invoice
                SET Status = CASE
                    WHEN ApprovedPrice IS NULL OR ApprovedPrice = 0
                        THEN 'NotApproved'
                    WHEN ApprovedPrice < RequestPrice
                        THEN 'PartiallyApproved'
                    WHEN ApprovedPrice = RequestPrice
                        THEN 'FullyApproved'
                    ELSE 'NotApproved'
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelCode",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelCode",
                table: "invoice");
        }
    }
}
