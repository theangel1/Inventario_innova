using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class updateProductoIdInProductoSolicitadoTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaOperacion",
                table: "ProductoSolicitado",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ProductoSolicitado",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductoSolicitado_UserId",
                table: "ProductoSolicitado",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductoSolicitado_AspNetUsers_UserId",
                table: "ProductoSolicitado",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductoSolicitado_AspNetUsers_UserId",
                table: "ProductoSolicitado");

            migrationBuilder.DropIndex(
                name: "IX_ProductoSolicitado_UserId",
                table: "ProductoSolicitado");

            migrationBuilder.DropColumn(
                name: "FechaOperacion",
                table: "ProductoSolicitado");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ProductoSolicitado");
        }
    }
}
