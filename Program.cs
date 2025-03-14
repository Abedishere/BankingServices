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

// 5. Add Controllers
builder.Services.AddControllers();

// 6. Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Banking Services API", 
        Version = "v1",
        Description = "API for managing banking transaction logs and account data"
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

// (Optional) Use Authorization middleware if you have secured endpoints
app.UseAuthorization();

app.UseRouting();

// 9. Initialize the Database (with robust error handling)
try
{
    // Create a new scope to resolve services
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<BankingDbContext>();
            
            // Test the connection before initializing
            if (context.Database.CanConnect())
            {
                // Use EnsureCreated for simplicity (or replace with context.Database.Migrate() if needed)
                context.Database.EnsureCreated();

                // Seed data only if needed
                if (context.Database.IsRelational())
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Seeding the database...");

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
            else
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError("Unable to connect to the database.");
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

// 10. Map Controllers
app.MapControllers();

// 11. Run the app
app.Run();
