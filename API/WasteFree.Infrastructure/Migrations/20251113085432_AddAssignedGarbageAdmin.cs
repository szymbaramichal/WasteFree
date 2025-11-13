using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteFree.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedGarbageAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedGarbageAdminId",
                table: "GarbageOrders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GarbageOrders_AssignedGarbageAdminId",
                table: "GarbageOrders",
                column: "AssignedGarbageAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_GarbageOrders_Users_AssignedGarbageAdminId",
                table: "GarbageOrders",
                column: "AssignedGarbageAdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GarbageOrders_Users_AssignedGarbageAdminId",
                table: "GarbageOrders");

            migrationBuilder.DropIndex(
                name: "IX_GarbageOrders_AssignedGarbageAdminId",
                table: "GarbageOrders");

            migrationBuilder.DropColumn(
                name: "AssignedGarbageAdminId",
                table: "GarbageOrders");
        }
    }
}
