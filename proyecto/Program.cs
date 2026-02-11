using Clasess;
using Clasess.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var dbPath = builder.Environment.IsDevelopment()
    ? Path.Combine(builder.Environment.ContentRootPath, "app.db")
    : "/home/site/wwwroot/app.db";

builder.Services.AddDbContext<SubDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}")
);

builder.Services.AddScoped<SubscriptionService>();

var app = builder.Build();

// (no hace falta que lo dejes permanentemente)
var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<SubDbContext>();
Console.WriteLine(db.Database.GetDbConnection().DataSource);

// Apply pending migrations and create the database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SubDbContext>();
    db.Database.Migrate(); 
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
