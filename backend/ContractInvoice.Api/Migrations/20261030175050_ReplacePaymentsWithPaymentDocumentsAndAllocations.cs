using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractInvoice.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePaymentsWithPaymentDocumentsAndAllocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.CreateTable(
                name: "PaymentDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentDocuments", x => x.Id);
                    table.CheckConstraint("CK_PaymentDocuments_TotalAmount", "[TotalAmount] > 0");
                });

            migrationBuilder.CreateTable(
                name: "PaymentAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentDocumentId = table.Column<int>(type: "int", nullable: false),
                    ProgressPaymentId = table.Column<int>(type: "int", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AllocationType = table.Column<int>(type: "int", nullable: false),
                    GoodPerformance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    InsuranceDeposit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ObligationsDeposit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    VatReceivable = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    RadiationAllowance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ForeignMission = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAllocations", x => x.Id);
                    table.CheckConstraint("CK_PaymentAllocations_AllocatedAmount", "[AllocatedAmount] > 0");
                    table.CheckConstraint("CK_PaymentAllocations_AllocationType", "[AllocationType] IN (1, 2)");
                    table.CheckConstraint("CK_PaymentAllocations_ForeignMission", "[ForeignMission] >= 0");
                    table.CheckConstraint("CK_PaymentAllocations_GoodPerformance", "[GoodPerformance] >= 0");
                    table.CheckConstraint("CK_PaymentAllocations_InsuranceDeposit", "[InsuranceDeposit] >= 0");
                    table.CheckConstraint("CK_PaymentAllocations_ObligationsDeposit", "[ObligationsDeposit] >= 0");
                    table.CheckConstraint("CK_PaymentAllocations_RadiationAllowance", "[RadiationAllowance] >= 0");
                    table.CheckConstraint("CK_PaymentAllocations_VatReceivable", "[VatReceivable] >= 0");
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_PaymentDocuments_PaymentDocumentId",
                        column: x => x.PaymentDocumentId,
                        principalTable: "PaymentDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_invoice_ProgressPaymentId",
                        column: x => x.ProgressPaymentId,
                        principalTable: "invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_PaymentDocumentId_ProgressPaymentId",
                table: "PaymentAllocations",
                columns: new[] { "PaymentDocumentId", "ProgressPaymentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_ProgressPaymentId",
                table: "PaymentAllocations",
                column: "ProgressPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDocuments_PaymentReferenceNo",
                table: "PaymentDocuments",
                column: "PaymentReferenceNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentAllocations");

            migrationBuilder.DropTable(
                name: "PaymentDocuments");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgressPaymentId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ForeignMission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GoodPerformance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InsuranceDeposit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ObligationsDeposit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RadiationAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReceivedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatReceivable = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_invoice_ProgressPaymentId",
                        column: x => x.ProgressPaymentId,
                        principalTable: "invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ProgressPaymentId",
                table: "Payments",
                column: "ProgressPaymentId");
        }
    }
}
