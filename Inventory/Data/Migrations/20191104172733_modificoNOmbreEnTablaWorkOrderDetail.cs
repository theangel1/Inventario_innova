using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class modificoNOmbreEnTablaWorkOrderDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TengoStock",
                table: "WorkOrder",
                newName: "SolicitoStock");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SolicitoStock",
                table: "WorkOrder",
                newName: "TengoStock");
        }
    }
}
