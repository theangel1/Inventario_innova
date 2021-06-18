using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class fixMovTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Movimiento",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movimiento_UserId",
                table: "Movimiento",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimiento_AspNetUsers_UserId",
                table: "Movimiento",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimiento_AspNetUsers_UserId",
                table: "Movimiento");

            migrationBuilder.DropIndex(
                name: "IX_Movimiento_UserId",
                table: "Movimiento");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Movimiento",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
