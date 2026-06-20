using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementAPI.Modules.Notifications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditActorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Notifications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Notifications");
        }
    }
}
