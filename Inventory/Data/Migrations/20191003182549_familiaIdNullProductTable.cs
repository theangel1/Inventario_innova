using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class familiaIdNullProductTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Familia_FamiliaId",
                table: "Producto");

            migrationBuilder.AlterColumn<int>(
                name: "FamiliaId",
                table: "Producto",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Familia_FamiliaId",
                table: "Producto",
                column: "FamiliaId",
                principalTable: "Familia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Familia_FamiliaId",
                table: "Producto");

            migrationBuilder.AlterColumn<int>(
                name: "FamiliaId",
                table: "Producto",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Familia_FamiliaId",
                table: "Producto",
                column: "FamiliaId",
                principalTable: "Familia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
