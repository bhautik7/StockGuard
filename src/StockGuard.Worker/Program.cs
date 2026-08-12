using Microsoft.EntityFrameworkCore;
using StockGuard.Application.Interfaces;
using StockGuard.Infrastructure.Persistence;
using StockGuard.Infrastructure.Repositories;
using StockGuard.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddHostedService<OutboxPublisherWorker>();
builder.Services.AddHostedService<ExpiryCheckWorker>();
builder.Services.AddSingleton(new AlertBroadcaster("http://localhost:5270/hubs/alerts"));
var host = builder.Build();
host.Run();