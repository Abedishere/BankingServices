using BankingServices.Configurations;
using BankingServices.Data;
using BankingServices.Services;
using MassTransit;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// 2. Add DbContext (PostgreSQL)
builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Configure OData
ODataConfig.ConfigureOData(builder.Services);

// 4. Register Services (DI)
builder.Services.AddScoped<ITransactionLogService, TransactionLogService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddScoped<IRabbitMQService, RabbitMQService>();

// 5. Configure MassTransit / RabbitMQ
builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });
    });
});

// 6. Add Controllers
builder.Services.AddControllers();

// 7. Build the app
var app = builder.Build();

// 8. Initialize the Database (Optional)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
    DbInitializer.Initialize(dbContext);
}

// 9. Configure the HTTP pipeline
app.UseRouting();
app.MapControllers();
app.Run();