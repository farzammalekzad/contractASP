using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractInvoice.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProgressPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgressPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    PaymentNo = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PaymentType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PeriodStartDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PeriodEndDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RequestLetterNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequestLetterDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ApprovalLetterNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovalDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ProposedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressPayments_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressPayments_ContractId_PaymentNo",
                table: "ProgressPayments",
                columns: new[] { "ContractId", "PaymentNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressPayments");
        }
    }
}
