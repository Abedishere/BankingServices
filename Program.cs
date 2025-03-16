using BankingServices.Configurations;
using BankingServices.Data;
using BankingServices.Events;
using BankingServices.Services;
using BankingServices.UnitOfWork;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

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
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEventService, EventService>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register MediatR using new syntax for MediatR 12
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(DomainEventNotification)));


// Register your domain event handler (optional if assembly scanning is sufficient)
builder.Services.AddTransient<INotificationHandler<DomainEventNotification>, DomainEventHandler>();

// Conditional registration for MassTransit services
if (builder.Configuration.GetValue<bool>("RabbitMQ:Enabled", false))
{
    builder.Services.AddMassTransit(config =>
    {
        config.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
            {
                // Use the default credentials ("guest" for both username and password)
                h.Username("guest");
                h.Password("guest");
            });
        });
    });
    
    builder.Services.AddScoped<ITransactionLogService, TransactionLogService>();
    builder.Services.AddScoped<IRabbitMQService, RabbitMQService>();
}
else
{
    builder.Services.AddScoped<ITransactionLogService>(sp =>
        new TransactionLogService(
            sp.GetRequiredService<BankingDbContext>(),
            sp.GetRequiredService<ILogger<TransactionLogService>>()));
    
    builder.Services.AddScoped<IRabbitMQService>(sp =>
        new RabbitMQService(
            sp.GetRequiredService<ILogger<RabbitMQService>>()));
}

// 5. Add Controllers
builder.Services.AddControllers();

// 6. Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Banking Services API", 
        Version = "v1",
        Description = "API for managing banking transaction logs, account data, and domain events"
    });
});

// 7. (Optional) Add Authorization Middleware if needed
builder.Services.AddAuthorization();

var app = builder.Build();

// 8. Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking Services API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseRouting();

// 9. Initialize the Database (with robust error handling)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<BankingDbContext>();

        if (context.Database.CanConnect())
        {
            context.Database.EnsureCreated();

            if (!context.TransactionLogs.Any())
            {
                context.TransactionLogs.Add(new BankingServices.Models.TransactionLog
                {
                    AccountId = 1,
                    TransactionType = "Deposit",
                    Amount = 1000.00m,
                    Timestamp = DateTime.UtcNow,
                    Status = "Completed",
                    Details = "Initial seed transaction"
                });
                context.SaveChanges();
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization error: {ex.Message}");
}

// 10. Map Controllers
app.MapControllers();

// 11. Run
app.Run();
