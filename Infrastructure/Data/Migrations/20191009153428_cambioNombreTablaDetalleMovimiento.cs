using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class cambioNombreTablaDetalleMovimiento : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleMovimiento_Movimiento_MovimientoId",
                table: "DetalleMovimiento");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleMovimiento_Producto_ProductoId",
                table: "DetalleMovimiento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalleMovimiento",
                table: "DetalleMovimiento");

            migrationBuilder.RenameTable(
                name: "DetalleMovimiento",
                newName: "MovimientoDetail");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleMovimiento_ProductoId",
                table: "MovimientoDetail",
                newName: "IX_MovimientoDetail_ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleMovimiento_MovimientoId",
                table: "MovimientoDetail",
                newName: "IX_MovimientoDetail_MovimientoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MovimientoDetail",
                table: "MovimientoDetail",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoDetail_Movimiento_MovimientoId",
                table: "MovimientoDetail",
                column: "MovimientoId",
                principalTable: "Movimiento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoDetail_Producto_ProductoId",
                table: "MovimientoDetail",
                column: "ProductoId",
                principalTable: "Producto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoDetail_Movimiento_MovimientoId",
                table: "MovimientoDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoDetail_Producto_ProductoId",
                table: "MovimientoDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MovimientoDetail",
                table: "MovimientoDetail");

            migrationBuilder.RenameTable(
                name: "MovimientoDetail",
                newName: "DetalleMovimiento");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientoDetail_ProductoId",
                table: "DetalleMovimiento",
                newName: "IX_DetalleMovimiento_ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientoDetail_MovimientoId",
                table: "DetalleMovimiento",
                newName: "IX_DetalleMovimiento_MovimientoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalleMovimiento",
                table: "DetalleMovimiento",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleMovimiento_Movimiento_MovimientoId",
                table: "DetalleMovimiento",
                column: "MovimientoId",
                principalTable: "Movimiento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleMovimiento_Producto_ProductoId",
                table: "DetalleMovimiento",
                column: "ProductoId",
                principalTable: "Producto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
