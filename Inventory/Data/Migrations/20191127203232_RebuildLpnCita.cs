using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class RebuildLpnCita : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_LPN_LpnID",
                table: "WorkOrder");

            migrationBuilder.DropTable(
                name: "LPN");

            migrationBuilder.RenameColumn(
                name: "LpnID",
                table: "WorkOrder",
                newName: "CitaID");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrder_LpnID",
                table: "WorkOrder",
                newName: "IX_WorkOrder_CitaID");

            migrationBuilder.AddColumn<string>(
                name: "Lpn",
                table: "WorkOrderDetail",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cita",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NumeroCita = table.Column<string>(nullable: true),
                    Patente = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cita", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_Cita_CitaID",
                table: "WorkOrder",
                column: "CitaID",
                principalTable: "Cita",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_Cita_CitaID",
                table: "WorkOrder");

            migrationBuilder.DropTable(
                name: "Cita");

            migrationBuilder.DropColumn(
                name: "Lpn",
                table: "WorkOrderDetail");

            migrationBuilder.RenameColumn(
                name: "CitaID",
                table: "WorkOrder",
                newName: "LpnID");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrder_CitaID",
                table: "WorkOrder",
                newName: "IX_WorkOrder_LpnID");

            migrationBuilder.CreateTable(
                name: "LPN",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Lpn = table.Column<string>(nullable: true),
                    NumeroCita = table.Column<string>(nullable: true),
                    Patente = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LPN", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_LPN_LpnID",
                table: "WorkOrder",
                column: "LpnID",
                principalTable: "LPN",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
