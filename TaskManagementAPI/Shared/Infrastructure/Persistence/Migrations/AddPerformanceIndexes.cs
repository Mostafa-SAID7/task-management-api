using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManagementAPI.Shared.Infrastructure.Persistence.Migrations;

/// <summary>
/// Migration to add performance-critical indexes for query optimization.
/// Addresses N+1 query problems and improves filtering performance.
/// </summary>
public partial class AddPerformanceIndexes : Migration
{
    /// <summary>
    /// Adds indexes to improve query performance.
    /// </summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // WorkTask indexes for common queries
        migrationBuilder.CreateIndex(
            name: "IX_WorkTask_ProjectId_Status",
            table: "Tasks",
            columns: new[] { "ProjectId", "Status" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_WorkTask_ProjectId_AssigneeId",
            table: "Tasks",
            columns: new[] { "ProjectId", "AssigneeId" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_WorkTask_Slug",
            table: "Tasks",
            column: "Slug",
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_WorkTask_ProjectId_CreatedAt",
            table: "Tasks",
            columns: new[] { "ProjectId", "CreatedAt" },
            filter: "[IsDeleted] = 0");

        // ProjectMember indexes
        migrationBuilder.CreateIndex(
            name: "IX_ProjectMember_ProjectId_IsDeleted",
            table: "ProjectMembers",
            columns: new[] { "ProjectId", "IsDeleted" });

        migrationBuilder.CreateIndex(
            name: "IX_ProjectMember_UserId",
            table: "ProjectMembers",
            column: "UserId",
            filter: "[IsDeleted] = 0");

        // TaskDependency indexes
        migrationBuilder.CreateIndex(
            name: "IX_TaskDependency_TaskId_BlockedByTaskId",
            table: "TaskDependencies",
            columns: new[] { "TaskId", "BlockedByTaskId" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_TaskDependency_BlockedByTaskId",
            table: "TaskDependencies",
            column: "BlockedByTaskId",
            filter: "[IsDeleted] = 0");

        // Notification indexes
        migrationBuilder.CreateIndex(
            name: "IX_Notification_UserId_IsRead",
            table: "Notifications",
            columns: new[] { "UserId", "IsRead" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_Notification_UserId_CreatedAt",
            table: "Notifications",
            columns: new[] { "UserId", "CreatedAt" },
            filter: "[IsDeleted] = 0");

        // Project indexes
        migrationBuilder.CreateIndex(
            name: "IX_Project_Slug",
            table: "Projects",
            column: "Slug",
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_Project_CreatedAt",
            table: "Projects",
            column: "CreatedAt",
            filter: "[IsDeleted] = 0");
    }

    /// <summary>
    /// Removes the added indexes.
    /// </summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_WorkTask_ProjectId_Status", table: "Tasks");
        migrationBuilder.DropIndex(name: "IX_WorkTask_ProjectId_AssigneeId", table: "Tasks");
        migrationBuilder.DropIndex(name: "IX_WorkTask_Slug", table: "Tasks");
        migrationBuilder.DropIndex(name: "IX_WorkTask_ProjectId_CreatedAt", table: "Tasks");
        migrationBuilder.DropIndex(name: "IX_ProjectMember_ProjectId_IsDeleted", table: "ProjectMembers");
        migrationBuilder.DropIndex(name: "IX_ProjectMember_UserId", table: "ProjectMembers");
        migrationBuilder.DropIndex(name: "IX_TaskDependency_TaskId_BlockedByTaskId", table: "TaskDependencies");
        migrationBuilder.DropIndex(name: "IX_TaskDependency_BlockedByTaskId", table: "TaskDependencies");
        migrationBuilder.DropIndex(name: "IX_Notification_UserId_IsRead", table: "Notifications");
        migrationBuilder.DropIndex(name: "IX_Notification_UserId_CreatedAt", table: "Notifications");
        migrationBuilder.DropIndex(name: "IX_Project_Slug", table: "Projects");
        migrationBuilder.DropIndex(name: "IX_Project_CreatedAt", table: "Projects");
    }
}
