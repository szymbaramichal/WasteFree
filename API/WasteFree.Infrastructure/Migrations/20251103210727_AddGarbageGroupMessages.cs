using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteFree.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGarbageGroupMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GarbageGroupMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GarbageGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarbageGroupMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarbageGroupMessages_GarbageGroups_GarbageGroupId",
                        column: x => x.GarbageGroupId,
                        principalTable: "GarbageGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GarbageGroupMessages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GarbageGroupMessages_GarbageGroupId_CreatedDateUtc",
                table: "GarbageGroupMessages",
                columns: new[] { "GarbageGroupId", "CreatedDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_GarbageGroupMessages_UserId",
                table: "GarbageGroupMessages",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GarbageGroupMessages");
        }
    }
}
