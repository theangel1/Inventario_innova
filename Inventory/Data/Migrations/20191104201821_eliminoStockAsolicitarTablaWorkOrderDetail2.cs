using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class eliminoStockAsolicitarTablaWorkOrderDetail2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SolicitoStock",
                table: "WorkOrderDetail");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SolicitoStock",
                table: "WorkOrderDetail",
                nullable: false,
                defaultValue: false);
        }
    }
}
