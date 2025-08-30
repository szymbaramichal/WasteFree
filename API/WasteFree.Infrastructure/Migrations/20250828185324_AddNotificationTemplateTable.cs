using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteFree.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTemplateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "NotificationTemplates",
                columns: new[] { "Id", "Subject", "Body", "Channel", "Type", "CreatedDateUtc", "CreatedBy", "ModifiedDateUtc", "ModifiedBy" },
                values: new object[] {
                    new Guid("11111111-1111-1111-1111-111111111111"),
                    "Welcome to WasteFree!",
                    @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f9f9f9;padding:40px;'>
                        <div style='max-width:600px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px #ccc;padding:32px;'>
                          <h2 style='color:#2e7d32;'>Welcome {{Username}} to WasteFree!</h2>
                          <p>
                            Thank you for registering. We are excited to have you join our community.<br><br>
                            <b>Get started by exploring our features and reducing waste today!</b>
                          </p>
                          <hr style='margin:24px 0;'>
                          <p style='font-size:12px;color:#888;'>
                            If you did not register, please ignore this email.
                          </p>
                        </div>
                      </body>
                    </html>",
                    0, // Channel: Email
                    0, // Type: RegisterationConfirmation
                    DateTime.UtcNow,
                    new Guid("00000000-0000-0000-0000-000000000000"),
                    null,
                    null
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationTemplates");
            // Remove inserted template if table still exists
            migrationBuilder.Sql("DELETE FROM NotificationTemplates WHERE Id = '11111111-1111-1111-1111-111111111111'");
        }
    }
}
