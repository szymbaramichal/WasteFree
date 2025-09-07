using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteFree.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguagePreferencesToTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LanguagePreference",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LanguagePreference",
                table: "NotificationTemplates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LanguagePreference",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LanguagePreference",
                table: "NotificationTemplates");
        }
    }
}
