using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class modtabladetalleot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadDevuelta",
                table: "WorkOrderDetail");

            migrationBuilder.AddColumn<bool>(
                name: "LpnSet",
                table: "WorkOrderDetail",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LpnSet",
                table: "WorkOrderDetail");

            migrationBuilder.AddColumn<int>(
                name: "CantidadDevuelta",
                table: "WorkOrderDetail",
                nullable: false,
                defaultValue: 0);
        }
    }
}
