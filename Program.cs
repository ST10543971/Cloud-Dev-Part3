using Microsoft.EntityFrameworkCore;
using VenueBookingApp.Data;
using VenueBookingApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";

if (string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
{
    var connectionString = builder.Configuration.GetConnectionString("AzureSqlConnection")
        ?? throw new InvalidOperationException("Azure SQL connection string is not configured.");

    builder.Services.AddDbContext<VenueBookingContext>(options => options.UseSqlServer(connectionString));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Default database connection string is not configured.");

    builder.Services.AddDbContext<VenueBookingContext>(options => options.UseSqlite(connectionString));
}

var app = builder.Build();

// Azure Blob storage registration removed

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VenueBookingContext>();
    await DbInitializer.InitializeAsync(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
