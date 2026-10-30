using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractInvoice.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentFinancialFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentReferenceNo",
                table: "Payments");

            migrationBuilder.AddColumn<decimal>(
                name: "ForeignMission",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GoodPerformance",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceDeposit",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ObligationsDeposit",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RadiationAllowance",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatReceivable",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForeignMission",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "GoodPerformance",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "InsuranceDeposit",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ObligationsDeposit",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RadiationAllowance",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VatReceivable",
                table: "Payments");

            migrationBuilder.AddColumn<string>(
                name: "PaymentReferenceNo",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
