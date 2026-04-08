using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementAPI.Modules.Tasks.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskDependencyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Tasks",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTask_ProjectId_AssigneeId",
                table: "Tasks",
                columns: new[] { "ProjectId", "AssigneeId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkTask_ProjectId_Status",
                table: "Tasks",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkTask_Slug",
                table: "Tasks",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDependency_TaskId_BlockedByTaskId",
                table: "TaskDependencies",
                columns: new[] { "TaskId", "BlockedByTaskId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkTask_ProjectId_AssigneeId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_WorkTask_ProjectId_Status",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_WorkTask_Slug",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_TaskDependency_TaskId_BlockedByTaskId",
                table: "TaskDependencies");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
