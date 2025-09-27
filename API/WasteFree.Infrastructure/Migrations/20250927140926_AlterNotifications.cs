using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteFree.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlterNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "InboxNotifications");

            migrationBuilder.AddColumn<bool>(
                name: "IsPending",
                table: "UserGarbageGroups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPending",
                table: "UserGarbageGroups");

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "InboxNotifications",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
