using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class FixToLPNRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_LPN_LPNId",
                table: "WorkOrder");

            migrationBuilder.RenameColumn(
                name: "LPNId",
                table: "WorkOrder",
                newName: "LpnID");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrder_LPNId",
                table: "WorkOrder",
                newName: "IX_WorkOrder_LpnID");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_LPN_LpnID",
                table: "WorkOrder",
                column: "LpnID",
                principalTable: "LPN",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_LPN_LpnID",
                table: "WorkOrder");

            migrationBuilder.RenameColumn(
                name: "LpnID",
                table: "WorkOrder",
                newName: "LPNId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrder_LpnID",
                table: "WorkOrder",
                newName: "IX_WorkOrder_LPNId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_LPN_LPNId",
                table: "WorkOrder",
                column: "LPNId",
                principalTable: "LPN",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
