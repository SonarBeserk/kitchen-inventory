using Microsoft.Data.Sqlite;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Handle dependency injection
builder.Services.AddSingleton<SqliteConnection>(_ =>
{
    var sqlSetting = builder.Configuration.GetConnectionString("sqlite");
    if (string.IsNullOrWhiteSpace(sqlSetting))
    {
        throw new InvalidOperationException("Database connection string is missing or invalid");
    }

    var conn = new SqliteConnection(sqlSetting);
    conn.Open(); // Open long-running connection to save memory and io calls
    return conn;
});
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<ILocationService, LocationService>();
builder.Services.AddSingleton<IAccountService, AccountService>();

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

// Migrate the database
SqlService.UpdateSchema(builder.Configuration.GetConnectionString("sqlite") ?? string.Empty);

app.Run();
