using DataStatisticsService.Gateway.Host.GraphQL;
using DataStatisticsService.Data;
using DataStatisticsService.Service;
using HotChocolate.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration).WriteTo.Console());

builder.Services
    .AddServiceLayer()
    .AddDataLayer(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddFiltering()
    .AddSorting();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapGraphQL().WithOptions(new HotChocolate.AspNetCore.GraphQLServerOptions
    {
        Tool = { Enable = true }
    });
}
else
{
    app.MapGraphQL();
}

app.UseSerilogRequestLogging();
app.UseCors("Frontend");
app.MapControllers();
app.MapGet("/health/live", () => Results.Ok());

app.Run();

public partial class Program;
