using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class addFieldsIsReasignedAndOrdenTrabajoReasignadaId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReasigned",
                table: "WorkOrder",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrdenTrabajoReasignadaID",
                table: "WorkOrder",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReasigned",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "OrdenTrabajoReasignadaID",
                table: "WorkOrder");
        }
    }
}
