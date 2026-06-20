using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementAPI.Modules.Tasks.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditActorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TimeTrackingEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "TimeTrackingEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TimeTrackingEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TaskDependencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "TaskDependencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TaskDependencies",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TimeTrackingEntries");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TimeTrackingEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TimeTrackingEntries");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TaskDependencies");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TaskDependencies");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TaskDependencies");
        }
    }
}
