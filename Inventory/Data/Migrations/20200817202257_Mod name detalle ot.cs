using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class Modnamedetalleot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CantidadRecepcionada",
                table: "WorkOrderDetail",
                newName: "CantidadDevuelta");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CantidadDevuelta",
                table: "WorkOrderDetail",
                newName: "CantidadRecepcionada");
        }
    }
}
