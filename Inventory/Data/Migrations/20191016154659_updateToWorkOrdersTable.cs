using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class updateToWorkOrdersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FacturaAsociada",
                table: "WorkOrder",
                newName: "NumeroFacturaRetail");

            migrationBuilder.AddColumn<int>(
                name: "NumeroFacturaExterno",
                table: "WorkOrder",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroFacturaExterno",
                table: "WorkOrder");

            migrationBuilder.RenameColumn(
                name: "NumeroFacturaRetail",
                table: "WorkOrder",
                newName: "FacturaAsociada");
        }
    }
}
