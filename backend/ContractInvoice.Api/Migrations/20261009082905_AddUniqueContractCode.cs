using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractInvoice.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueContractCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex
            (
            name: "IX_Contracts_ContractCode",
            table: "Contracts",
            column: "ContractCode",
            unique: true
            );

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
            name: "IX_Contracts_ContractCode",
            table: "Contracts");

        }
    }
}
