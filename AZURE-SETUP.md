# Venue Booking App Setup Guide

## What is already implemented

- ASP.NET Core MVC models for venues, events, and bookings
- Entity Framework Core context and relationships
- CRUD controllers and views for all three entities
- Initial migration for the database schema
- SQLite configured for local development by default
- Azure SQL connection string placeholder in `appsettings.json`
- Startup now applies EF Core migrations automatically on app start

## Local run

1. Build the app:

   ```powershell
   dotnet build
   ```

2. Apply the database migration:

   ```powershell
   dotnet ef database update
   ```

3. Start the app:

   ```powershell
   dotnet run
   ```

The local SQLite database file is created as `venuebooking.db` in the project folder.

## Switch from SQLite to Azure SQL

### Recommended resource names

- Resource group: `rg-lm-dev-venuebooking-sa`
- SQL server: `sql-lm-dev-venuebooking-sa`
- SQL database: `sqldb-lm-dev-venuebooking`
- App Service plan: `asp-lm-dev-venuebooking`
- Web app: `app-lm-dev-venuebooking`
- Region: `South Africa North`

Adjust these if Azure reports that a name is already taken.

### Create Azure SQL in the portal

1. Sign in to the Azure portal.
2. Create a resource group, for example `rg-lm-dev-venuebooking-sa`.
3. Create an Azure SQL server in `South Africa North`.
4. Create an Azure SQL database on that server.
5. In the SQL server networking settings, allow your client IP address.
6. Copy the ADO.NET connection string from the Azure SQL Database connection strings page.

### Update the app for Azure SQL

1. Replace the `AzureSqlConnection` value in `appsettings.json` with your real Azure SQL connection string if you want to test it locally.
2. Change:

   ```json
   "DatabaseProvider": "Sqlite"
   ```

   to:

   ```json
   "DatabaseProvider": "SqlServer"
   ```

3. Apply the same EF migration to Azure SQL:

   ```powershell
   dotnet ef database update
   ```

Because startup now uses `Database.MigrateAsync()`, the app can also apply pending migrations automatically when it starts against an empty Azure SQL database.

## Move local data to Azure SQL

If you captured data locally in SQLite first, move it like this:

1. Keep `DatabaseProvider` as `Sqlite` and run the app.
2. Record or export the existing data from Venues, Events, and Bookings.
3. Switch `DatabaseProvider` to `SqlServer`.
4. Run `dotnet ef database update` again to create the Azure SQL tables.
5. Re-enter the local records through the web interface, or script a once-off import if your lecturer expects automated migration.

For a small class project, re-entering seeded or sample records through the UI is usually enough unless the rubric explicitly requires a scripted data transfer.

## Publish to Azure App Service

### Recommended VS Code extensions

```vscode-extensions
ms-azuretools.vscode-azureresourcegroups,ms-azuretools.vscode-azureappservice,ms-mssql.mssql
```

### Publish using VS Code

1. Install the Azure extensions above.
2. Sign in to Azure from VS Code.
3. Create an App Service plan, for example `asp-lm-dev-venuebooking`.
4. Create a Web App, for example `app-lm-dev-venuebooking`.
5. Publish this project to that Web App from VS Code.

### App Service configuration

In the App Service configuration settings, add:

   - `DatabaseProvider` = `SqlServer`
   - `ConnectionStrings__AzureSqlConnection` = your Azure SQL connection string

Then restart the App Service.

### Verify the deployed site

1. Browse to the deployed web app URL.
2. Confirm the dashboard loads.
3. Confirm `Venues`, `Events`, and `Bookings` pages load.
4. Create at least one record to prove database write access is working.

## Suggested screenshots for submission

- Home dashboard
- Venues list and create form
- Events list and create form
- Bookings list and create form
- Azure SQL Database creation page
- Azure SQL server networking or firewall page
- Azure App Service deployment page
- Running published web app