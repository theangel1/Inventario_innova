using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class addSomePropToProductTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Producto");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Producto",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SKU",
                table: "Producto",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "SKU",
                table: "Producto");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Producto",
                nullable: false,
                defaultValue: false);
        }
    }
}
