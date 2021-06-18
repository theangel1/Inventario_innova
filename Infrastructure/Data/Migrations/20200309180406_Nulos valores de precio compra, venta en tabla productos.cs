using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory.Data.Migrations
{
    public partial class Nulosvaloresdepreciocompraventaentablaproductos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Producto_Detalle");

            migrationBuilder.DropTable(
                name: "ProductoSolicitado");

            migrationBuilder.AlterColumn<double>(
                name: "PrecioVenta",
                table: "Producto",
                nullable: true,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<double>(
                name: "PrecioCompra",
                table: "Producto",
                nullable: true,
                oldClrType: typeof(double));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "PrecioVenta",
                table: "Producto",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "PrecioCompra",
                table: "Producto",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Producto_Detalle",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DetalleID = table.Column<int>(nullable: false),
                    LPN = table.Column<string>(nullable: true),
                    ProductoID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producto_Detalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Producto_Detalle_WorkOrderDetail_DetalleID",
                        column: x => x.DetalleID,
                        principalTable: "WorkOrderDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Producto_Detalle_Producto_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductoSolicitado",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Cantidad = table.Column<int>(nullable: false),
                    FechaOperacion = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ProductoId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    WorkOrderId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoSolicitado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductoSolicitado_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductoSolicitado_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductoSolicitado_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Producto_Detalle_DetalleID",
                table: "Producto_Detalle",
                column: "DetalleID");

            migrationBuilder.CreateIndex(
                name: "IX_Producto_Detalle_ProductoID",
                table: "Producto_Detalle",
                column: "ProductoID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoSolicitado_ProductoId",
                table: "ProductoSolicitado",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoSolicitado_UserId",
                table: "ProductoSolicitado",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoSolicitado_WorkOrderId",
                table: "ProductoSolicitado",
                column: "WorkOrderId");
        }
    }
}
