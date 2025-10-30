using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteFree.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGarbageOrdersInitialTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GarbageOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PickupOption = table.Column<int>(type: "INTEGER", nullable: false),
                    ContainerSize = table.Column<int>(type: "INTEGER", nullable: true),
                    DropOffDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PickupDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsHighPriority = table.Column<bool>(type: "INTEGER", nullable: false),
                    CollectingService = table.Column<bool>(type: "INTEGER", nullable: false),
                    GarbageOrderStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    GarbageGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarbageOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarbageOrders_GarbageGroups_GarbageGroupId",
                        column: x => x.GarbageGroupId,
                        principalTable: "GarbageGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GarbageOrderUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GarbageOrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HasAcceptedPayment = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarbageOrderUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarbageOrderUsers_GarbageOrders_GarbageOrderId",
                        column: x => x.GarbageOrderId,
                        principalTable: "GarbageOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GarbageOrderUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GarbageOrders_GarbageGroupId",
                table: "GarbageOrders",
                column: "GarbageGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GarbageOrderUsers_GarbageOrderId",
                table: "GarbageOrderUsers",
                column: "GarbageOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GarbageOrderUsers_UserId",
                table: "GarbageOrderUsers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GarbageOrderUsers");

            migrationBuilder.DropTable(
                name: "GarbageOrders");
        }
    }
}
