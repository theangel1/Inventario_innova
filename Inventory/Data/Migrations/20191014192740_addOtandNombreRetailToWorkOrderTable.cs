using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class addOtandNombreRetailToWorkOrderTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NombreRetail",
                table: "WorkOrder",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrdenCompra",
                table: "WorkOrder",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombreRetail",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "OrdenCompra",
                table: "WorkOrder");
        }
    }
}
