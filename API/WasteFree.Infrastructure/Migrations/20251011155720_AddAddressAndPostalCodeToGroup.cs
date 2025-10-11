using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteFree.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressAndPostalCodeToGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "GarbageGroups",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "GarbageGroups",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "GarbageGroups");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "GarbageGroups");
        }
    }
}
