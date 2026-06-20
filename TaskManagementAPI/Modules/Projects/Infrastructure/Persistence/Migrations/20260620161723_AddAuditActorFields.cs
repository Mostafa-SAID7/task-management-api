using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementAPI.Modules.Projects.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditActorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ProjectMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ProjectMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ProjectMembers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ProjectMembers");
        }
    }
}
