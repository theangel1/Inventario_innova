using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class addEmpresaToOT : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "WorkOrder",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_EmpresaId",
                table: "WorkOrder",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_Empresa_EmpresaId",
                table: "WorkOrder",
                column: "EmpresaId",
                principalTable: "Empresa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_Empresa_EmpresaId",
                table: "WorkOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrder_EmpresaId",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "WorkOrder");
        }
    }
}
