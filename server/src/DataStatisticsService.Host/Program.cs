using DataStatisticsService.Host.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapServiceDefaults();

app.Run();
