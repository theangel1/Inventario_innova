using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class addMovimientoDetailId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimiento_Producto_ProductoId",
                table: "Movimiento");

            migrationBuilder.DropIndex(
                name: "IX_Movimiento_ProductoId",
                table: "Movimiento");

            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "Movimiento");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "Movimiento");

            migrationBuilder.DropColumn(
                name: "Saldo",
                table: "Movimiento");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Cantidad",
                table: "Movimiento",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "Movimiento",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Saldo",
                table: "Movimiento",
                nullable: false,
                defaultValue: 0.0);

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
                onDelete: ReferentialAction.Cascade);
        }
    }
}
