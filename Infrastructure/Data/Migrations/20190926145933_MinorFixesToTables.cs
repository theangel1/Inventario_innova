using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class MinorFixesToTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "Movimiento",
                newName: "FechaMovimiento");

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "Movimiento",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Movimiento_ProductoId",
                table: "Movimiento",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimiento_Producto_ProductoId",
                table: "Movimiento",
                column: "ProductoId",
                principalTable: "Producto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimiento_Producto_ProductoId",
                table: "Movimiento");

            migrationBuilder.DropIndex(
                name: "IX_Movimiento_ProductoId",
                table: "Movimiento");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "Movimiento");

            migrationBuilder.RenameColumn(
                name: "FechaMovimiento",
                table: "Movimiento",
                newName: "FechaCreacion");
        }
    }
}
