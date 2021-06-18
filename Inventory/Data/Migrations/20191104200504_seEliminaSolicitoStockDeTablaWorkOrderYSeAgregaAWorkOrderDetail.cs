using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class seEliminaSolicitoStockDeTablaWorkOrderYSeAgregaAWorkOrderDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SolicitoStock",
                table: "WorkOrder");

            migrationBuilder.AddColumn<bool>(
                name: "SolicitoStock",
                table: "WorkOrderDetail",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SolicitoStock",
                table: "WorkOrderDetail");

            migrationBuilder.AddColumn<bool>(
                name: "SolicitoStock",
                table: "WorkOrder",
                nullable: false,
                defaultValue: false);
        }
    }
}
