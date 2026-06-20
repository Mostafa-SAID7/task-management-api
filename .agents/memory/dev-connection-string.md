---
name: Dev connection string bug
description: appsettings.Development.json had a SQL Server connection string that overrode the PostgreSQL one from appsettings.json
---

The `appsettings.Development.json` file originally had:
```
"DefaultConnection": "Server=.;Database=TaskManagementAPI_Dev;Trusted_Connection=true;TrustServerCertificate=true;"
```

This overrode the valid PostgreSQL connection string in `appsettings.json`. Since the app runs in the Development environment on Replit, every migration and DB call failed with "Couldn't set trusted_connection" — Npgsql doesn't support that parameter.

**Why:** The project was originally scaffolded for SQL Server and never updated for the Replit PostgreSQL environment.

**How to apply:** If migrations start failing with "trusted_connection" errors, check `appsettings.Development.json` first. The correct connection string for Replit is:
`Host=helium;Port=5432;Database=heliumdb;Username=postgres;Password=password;SSL Mode=Disable`
