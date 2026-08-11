using Microsoft.EntityFrameworkCore;
using StockGuard.Application.Interfaces;
using StockGuard.Infrastructure.Persistence;
using StockGuard.Infrastructure.Repositories;
using StockGuard.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddHostedService<ExpiryCheckWorker>();

var host = builder.Build();
host.Run();