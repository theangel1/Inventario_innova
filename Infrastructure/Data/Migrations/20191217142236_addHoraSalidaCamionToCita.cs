using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class addHoraSalidaCamionToCita : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OweProducto",
                table: "WorkOrder");

            migrationBuilder.AddColumn<DateTime>(
                name: "HoraSalidaCamion",
                table: "Cita",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoraSalidaCamion",
                table: "Cita");

            migrationBuilder.AddColumn<bool>(
                name: "OweProducto",
                table: "WorkOrder",
                nullable: false,
                defaultValue: false);
        }
    }
}
