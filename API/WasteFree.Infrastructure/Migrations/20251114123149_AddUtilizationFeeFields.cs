using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteFree.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUtilizationFeeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalUtilizationFeeShareAmount",
                table: "GarbageOrderUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalUtilizationFeeAmount",
                table: "GarbageOrders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UtilizationFeeAmount",
                table: "GarbageOrders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UtilizationFeeSubmittedDateUtc",
                table: "GarbageOrders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UtilizationProofBlobName",
                table: "GarbageOrders",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalUtilizationFeeShareAmount",
                table: "GarbageOrderUsers");

            migrationBuilder.DropColumn(
                name: "AdditionalUtilizationFeeAmount",
                table: "GarbageOrders");

            migrationBuilder.DropColumn(
                name: "UtilizationFeeAmount",
                table: "GarbageOrders");

            migrationBuilder.DropColumn(
                name: "UtilizationFeeSubmittedDateUtc",
                table: "GarbageOrders");

            migrationBuilder.DropColumn(
                name: "UtilizationProofBlobName",
                table: "GarbageOrders");
        }
    }
}
