using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class updateTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Empresa_EmpresaId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Empresa_EmpresaId",
                table: "Producto");

            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Familia_FamiliaId",
                table: "Producto");

            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Movimiento_MovimientoId",
                table: "Producto");

            migrationBuilder.DropIndex(
                name: "IX_Producto_MovimientoId",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "MovimientoId",
                table: "Producto");

            migrationBuilder.AlterColumn<int>(
                name: "FamiliaId",
                table: "Producto",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Producto",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

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
                name: "FK_AspNetUsers_Empresa_EmpresaId",
                table: "AspNetUsers",
                column: "EmpresaId",
                principalTable: "Empresa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Movimiento_Producto_ProductoId",
                table: "Movimiento",
                column: "ProductoId",
                principalTable: "Producto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Empresa_EmpresaId",
                table: "Producto",
                column: "EmpresaId",
                principalTable: "Empresa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Familia_FamiliaId",
                table: "Producto",
                column: "FamiliaId",
                principalTable: "Familia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Empresa_EmpresaId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Movimiento_Producto_ProductoId",
                table: "Movimiento");

            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Empresa_EmpresaId",
                table: "Producto");

            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Familia_FamiliaId",
                table: "Producto");

            migrationBuilder.DropIndex(
                name: "IX_Movimiento_ProductoId",
                table: "Movimiento");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "Movimiento");

            migrationBuilder.AlterColumn<int>(
                name: "FamiliaId",
                table: "Producto",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Producto",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "MovimientoId",
                table: "Producto",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Producto_MovimientoId",
                table: "Producto",
                column: "MovimientoId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Empresa_EmpresaId",
                table: "AspNetUsers",
                column: "EmpresaId",
                principalTable: "Empresa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Empresa_EmpresaId",
                table: "Producto",
                column: "EmpresaId",
                principalTable: "Empresa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Familia_FamiliaId",
                table: "Producto",
                column: "FamiliaId",
                principalTable: "Familia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Movimiento_MovimientoId",
                table: "Producto",
                column: "MovimientoId",
                principalTable: "Movimiento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
