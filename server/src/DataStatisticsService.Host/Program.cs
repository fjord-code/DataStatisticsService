using DataStatisticsService.Host.Extensions;
using DataStatisticsService.Data.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StatisticsDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapServiceDefaults();

app.Run();
