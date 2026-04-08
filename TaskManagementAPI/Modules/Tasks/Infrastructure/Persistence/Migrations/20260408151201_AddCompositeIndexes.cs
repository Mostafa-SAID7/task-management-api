using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementAPI.Modules.Tasks.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkTask_ProjectId_Status",
                table: "WorkTasks",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkTask_ProjectId_AssigneeId",
                table: "WorkTasks",
                columns: new[] { "ProjectId", "AssigneeId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkTask_Slug",
                table: "WorkTasks",
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
                name: "IX_WorkTask_ProjectId_Status",
                table: "WorkTasks");

            migrationBuilder.DropIndex(
                name: "IX_WorkTask_ProjectId_AssigneeId",
                table: "WorkTasks");

            migrationBuilder.DropIndex(
                name: "IX_WorkTask_Slug",
                table: "WorkTasks");

            migrationBuilder.DropIndex(
                name: "IX_TaskDependency_TaskId_BlockedByTaskId",
                table: "TaskDependencies");
        }
    }
}
