using System.Data;
using Microsoft.Data.Sqlite;
using Web.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Handle dependency injection
builder.Services.AddSingleton<SqliteConnection>(options =>
{
    var sqlSetting = builder.Configuration.GetConnectionString("sqlite");
    if (string.IsNullOrWhiteSpace(sqlSetting))
    {
        throw new InvalidOperationException("Database connection string is missing or invalid");
    }
    return new SqliteConnection(sqlSetting);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
