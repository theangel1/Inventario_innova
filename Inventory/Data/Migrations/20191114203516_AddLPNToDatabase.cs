using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class AddLPNToDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LPNId",
                table: "WorkOrder",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LPN",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Lpn = table.Column<string>(nullable: true),
                    NumeroCita = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LPN", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_LPNId",
                table: "WorkOrder",
                column: "LPNId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_LPN_LPNId",
                table: "WorkOrder",
                column: "LPNId",
                principalTable: "LPN",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_LPN_LPNId",
                table: "WorkOrder");

            migrationBuilder.DropTable(
                name: "LPN");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrder_LPNId",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "LPNId",
                table: "WorkOrder");
        }
    }
}
