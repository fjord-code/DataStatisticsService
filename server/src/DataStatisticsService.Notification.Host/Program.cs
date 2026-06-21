using DataStatisticsService.Abstractions.Messaging;
using DataStatisticsService.Data.Configuration;
using DataStatisticsService.Notification.Host.Hubs;
using DataStatisticsService.Notification.Host.Messaging;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration).WriteTo.Console());

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var rabbitMqOptions = builder.Configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions();

builder.Host.UseWolverine(options =>
{
    options.Discovery.IncludeAssembly(typeof(StatisticsUpdatedMessageHandler).Assembly);

    options
        .UseRabbitMq(c =>
        {
            c.HostName = rabbitMqOptions.HostName;
            c.Port = rabbitMqOptions.Port;
            c.UserName = rabbitMqOptions.UserName;
            c.Password = rabbitMqOptions.Password;
        })
        .AutoProvision();

    options.ListenToRabbitQueue(rabbitMqOptions.StatisticsUpdatesQueue)
        .DefaultIncomingMessage<StatisticsUpdatedMessage>();
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseCors("Frontend");
app.MapHub<StatisticsHub>("/hubs/statistics");
app.MapGet("/health/live", () => Results.Ok());

app.Run();

public partial class Program;
