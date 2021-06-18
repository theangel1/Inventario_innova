using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class testIcollectionOnMovimiento : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimiento_Producto_ProductoId",
                table: "Movimiento");

            migrationBuilder.DropIndex(
                name: "IX_Movimiento_ProductoId",
                table: "Movimiento");

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "Producto",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Producto_ProductoId",
                table: "Producto",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Movimiento_ProductoId",
                table: "Producto",
                column: "ProductoId",
                principalTable: "Movimiento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Movimiento_ProductoId",
                table: "Producto");

            migrationBuilder.DropIndex(
                name: "IX_Producto_ProductoId",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "Producto");

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
