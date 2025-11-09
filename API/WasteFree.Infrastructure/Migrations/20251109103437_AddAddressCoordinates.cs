using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteFree.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Address_Latitude",
                table: "Users",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Address_Longitude",
                table: "Users",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Address_Latitude",
                table: "GarbageGroups",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Address_Longitude",
                table: "GarbageGroups",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_Latitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Address_Longitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Address_Latitude",
                table: "GarbageGroups");

            migrationBuilder.DropColumn(
                name: "Address_Longitude",
                table: "GarbageGroups");
        }
    }
}
