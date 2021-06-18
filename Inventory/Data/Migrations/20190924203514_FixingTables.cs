using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class FixingTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoProducto",
                table: "Producto",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Cantidad",
                table: "Movimiento",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TipoOperacion",
                table: "Movimiento",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoProducto",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "Movimiento");

            migrationBuilder.DropColumn(
                name: "TipoOperacion",
                table: "Movimiento");
        }
    }
}
