using System.Text.Json.Serialization;
using ArchiSubscription.API.Middleware;
using ArchiSubscription.Core.Validators;
using ArchiSubscription.Infrastructure;
using ArchiSubscription.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Suppress noisy EF Core SQL logs ─────────────────────────────────────────
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.Warning);

// ─── Controllers & JSON Configuration ────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ─── Swagger / OpenAPI ───────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Archi Subscription API",
        Version = "v1",
        Description = "Subscription & Automatic Payment Reminder Banking Application API"
    });
});

// ─── FluentValidation ────────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerValidator>();

// ─── Infrastructure (DB, Repositories, Services, External Services) ──────────
builder.Services.AddInfrastructure(builder.Configuration);

// ─── CORS ────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ─── Middleware Pipeline ─────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Archi Subscription API v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseCors("AllowAll");
app.MapControllers();

// ─── Database Initialization (Auto-create on startup for development) ────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var retries = 5;
    while (retries > 0)
    {
        try
        {
            db.Database.EnsureCreated();
            logger.LogInformation("Database initialized successfully.");
            break;
        }
        catch (Exception ex)
        {
            retries--;
            logger.LogWarning(ex, "Database not ready. Retries left: {Retries}", retries);
            if (retries == 0) throw;
            Thread.Sleep(3000);
        }
    }
}

app.Run();
