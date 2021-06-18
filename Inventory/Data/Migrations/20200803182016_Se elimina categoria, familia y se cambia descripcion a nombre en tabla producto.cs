using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class Seeliminacategoriafamiliaysecambiadescripcionanombreentablaproducto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Familia_FamiliaId",
                table: "Producto");

            migrationBuilder.DropTable(
                name: "Familia");

            migrationBuilder.DropIndex(
                name: "IX_Producto_FamiliaId",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "FamiliaId",
                table: "Producto");

            migrationBuilder.RenameColumn(
                name: "UnidadMedida",
                table: "Producto",
                newName: "Nombre");

            migrationBuilder.AddColumn<bool>(
                name: "IsTerminado",
                table: "Producto",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTerminado",
                table: "Producto");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Producto",
                newName: "UnidadMedida");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Producto",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Producto",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FamiliaId",
                table: "Producto",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Familia",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FechaCreacion = table.Column<DateTime>(nullable: false),
                    Nombre = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Familia", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Producto_FamiliaId",
                table: "Producto",
                column: "FamiliaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Familia_FamiliaId",
                table: "Producto",
                column: "FamiliaId",
                principalTable: "Familia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
