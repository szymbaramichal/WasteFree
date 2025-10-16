using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteFree.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressValueObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "GarbageGroups");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Users",
                newName: "Address_City");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "GarbageGroups",
                newName: "Address_PostalCode");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "GarbageGroups",
                newName: "Address_City");

            migrationBuilder.AlterColumn<string>(
                name: "Address_City",
                table: "Users",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_PostalCode",
                table: "Users",
                type: "TEXT",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_Street",
                table: "Users",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_Street",
                table: "GarbageGroups",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_PostalCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Address_Street",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Address_Street",
                table: "GarbageGroups");

            migrationBuilder.RenameColumn(
                name: "Address_City",
                table: "Users",
                newName: "City");

            migrationBuilder.RenameColumn(
                name: "Address_PostalCode",
                table: "GarbageGroups",
                newName: "PostalCode");

            migrationBuilder.RenameColumn(
                name: "Address_City",
                table: "GarbageGroups",
                newName: "City");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Users",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "GarbageGroups",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
