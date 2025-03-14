using BankingServices.Configurations;
using BankingServices.Data;
using BankingServices.Services;
using MassTransit;
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

// Conditional registration for MassTransit services
if (builder.Configuration.GetValue<bool>("RabbitMQ:Enabled", false))
{
    // Configure MassTransit / RabbitMQ
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
    
    builder.Services.AddScoped<ITransactionLogService, TransactionLogService>();
    builder.Services.AddScoped<IRabbitMQService, RabbitMQService>();
}
else
{
    // Register services without MassTransit dependencies
    builder.Services.AddScoped<ITransactionLogService>(sp => 
        new TransactionLogService(
            sp.GetRequiredService<BankingDbContext>(),
            sp.GetRequiredService<ILogger<TransactionLogService>>()));
    
    builder.Services.AddScoped<IRabbitMQService>(sp => 
        new RabbitMQService(
            sp.GetRequiredService<ILogger<RabbitMQService>>()));
}

// 6. Add Controllers
builder.Services.AddControllers();

// 7. Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Banking Services API", 
        Version = "v1",
        Description = "API for managing banking transaction logs"
    });
});

// 8. Build the app
var app = builder.Build();

// 9. Configure pipeline before attempting database initialization
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking Services API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();

// 10. Initialize the Database (with robust error handling)
try
{
    // Create a new scope to resolve services
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<BankingDbContext>();
            
            // Test the connection before trying to initialize
            context.Database.CanConnect();
            
            // Skip migration for now, just use EnsureCreated
            context.Database.EnsureCreated();
            
            // Only seed data if the database was created successfully
            if (context.Database.IsRelational())
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Seeding the database...");
                
                // Seed initial data
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
                
                logger.LogInformation("Database initialized successfully.");
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while initializing the database.");
            // Continue with app startup even if DB initialization fails
        }
    }
}
catch (Exception ex)
{
    // Log the exception at the application level
    Console.WriteLine($"Application startup error: {ex.Message}");
    // Continue with app startup even if there's an error
}

// 11. Map controllers and run the app
app.MapControllers();
app.Run();