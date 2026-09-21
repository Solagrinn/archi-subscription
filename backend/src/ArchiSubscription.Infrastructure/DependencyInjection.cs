using ArchiSubscription.Core.Interfaces.Repositories;
using ArchiSubscription.Core.Interfaces.Services;
using ArchiSubscription.Infrastructure.Data;
using ArchiSubscription.Infrastructure.ExternalServices;
using ArchiSubscription.Infrastructure.Repositories;
using ArchiSubscription.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiSubscription.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database – Microsoft SQL Server
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        // Repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Services
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReminderService, ReminderService>();

        // External (Mock) Services
        services.AddScoped<IDebtInquiryService, MockDebtInquiryService>();
        services.AddScoped<IPaymentProcessingService, MockPaymentProcessingService>();

        // Notification (Mock) Services
        services.AddScoped<INotificationService, MockNotificationService>();

        // Time Warp (singleton – shared virtual clock for testing)
        services.AddSingleton<TimeWarpService>();

        // Data Seeder (for testing/demo)
        services.AddScoped<DataSeederService>();

        return services;
    }
}

