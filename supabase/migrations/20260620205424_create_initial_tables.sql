-- Migration: Create initial tables for Task Management API

-- Projects Table
CREATE TABLE IF NOT EXISTS "Projects" (
    "Id" uuid NOT NULL,
    "Name" character varying(255) NOT NULL,
    "Description" text,
    "CreatedAt" timestamp without time zone NOT NULL,
    "UpdatedAt" timestamp without time zone NOT NULL,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255),
    "IsDeleted" boolean NOT NULL DEFAULT false,
    PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_Projects_IsDeleted" ON "Projects"("IsDeleted");

-- Tasks Table
CREATE TABLE IF NOT EXISTS "Tasks" (
    "Id" uuid NOT NULL,
    "Title" character varying(255) NOT NULL,
    "Description" text,
    "Status" integer NOT NULL,
    "Priority" integer NOT NULL DEFAULT 0,
    "ProjectId" uuid NOT NULL,
    "AssignedTo" uuid,
    "CreatedAt" timestamp without time zone NOT NULL,
    "UpdatedAt" timestamp without time zone NOT NULL,
    "CreatedBy" character varying(255),
    "UpdatedBy" character varying(255),
    "IsDeleted" boolean NOT NULL DEFAULT false,
    PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Tasks_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_Tasks_ProjectId" ON "Tasks"("ProjectId");
CREATE INDEX IF NOT EXISTS "IX_Tasks_IsDeleted" ON "Tasks"("IsDeleted");
CREATE INDEX IF NOT EXISTS "IX_Tasks_Status" ON "Tasks"("Status");

-- Users Table
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" uuid NOT NULL,
    "Email" character varying(255) NOT NULL,
    "FullName" character varying(255),
    "Role" character varying(50),
    "CreatedAt" timestamp without time zone NOT NULL,
    "UpdatedAt" timestamp without time zone NOT NULL,
    "LastLogin" timestamp without time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users"("Email");
CREATE INDEX IF NOT EXISTS "IX_Users_IsDeleted" ON "Users"("IsDeleted");

-- Notifications Table
CREATE TABLE IF NOT EXISTS "Notifications" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Message" text NOT NULL,
    "Type" character varying(50),
    "IsRead" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp without time zone NOT NULL,
    "ReadAt" timestamp without time zone,
    PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Notifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_Notifications_UserId" ON "Notifications"("UserId");
CREATE INDEX IF NOT EXISTS "IX_Notifications_IsRead" ON "Notifications"("IsRead");

-- Audit Log Table
CREATE TABLE IF NOT EXISTS "AuditLogs" (
    "Id" uuid NOT NULL,
    "EntityName" character varying(255),
    "EntityId" uuid,
    "Action" character varying(50),
    "OldValues" jsonb,
    "NewValues" jsonb,
    "UserId" uuid,
    "Timestamp" timestamp without time zone NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_AuditLogs_EntityId" ON "AuditLogs"("EntityId");
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_UserId" ON "AuditLogs"("UserId");
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_Timestamp" ON "AuditLogs"("Timestamp");

-- Task Blocking/Dependencies Table
CREATE TABLE IF NOT EXISTS "TaskBlockings" (
    "Id" uuid NOT NULL,
    "BlockedTaskId" uuid NOT NULL,
    "BlockingTaskId" uuid NOT NULL,
    "CreatedAt" timestamp without time zone NOT NULL,
    "CreatedBy" character varying(255),
    PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TaskBlockings_Tasks_BlockedTaskId" FOREIGN KEY ("BlockedTaskId") REFERENCES "Tasks"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_TaskBlockings_Tasks_BlockingTaskId" FOREIGN KEY ("BlockingTaskId") REFERENCES "Tasks"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_TaskBlockings_BlockedTaskId" ON "TaskBlockings"("BlockedTaskId");
CREATE INDEX IF NOT EXISTS "IX_TaskBlockings_BlockingTaskId" ON "TaskBlockings"("BlockingTaskId");

-- EF Migrations History Table
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    PRIMARY KEY ("MigrationId")
);

-- Insert migration history for tracking
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES 
    ('20260620205424_InitialCreate', '9.0.0'),
    ('20260620205425_AddAuditFields', '9.0.0')
ON CONFLICT DO NOTHING;
