using ArchiSubscription.Core.Entities;
using ArchiSubscription.Core.Enums;
using ArchiSubscription.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArchiSubscription.Infrastructure.Services;

/// <summary>
/// Seeds the database with realistic test data for demo/testing purposes.
/// Can be called multiple times — each call adds a new customer with subscriptions and payments.
/// </summary>
public class DataSeederService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataSeederService> _logger;
    private readonly TimeWarpService _timeWarp;
    private static readonly Random Rng = new();
    private static int _seedCounter;

    public DataSeederService(AppDbContext context, ILogger<DataSeederService> logger, TimeWarpService timeWarp)
    {
        _context = context;
        _logger = logger;
        _timeWarp = timeWarp;
    }

    public async Task<SeedResult> SeedAsync(CancellationToken ct = default)
    {
        var now = _timeWarp.UtcNow;
        var currentPeriod = now.ToString("yyyy-MM");
        var lastMonth = now.AddMonths(-1).ToString("yyyy-MM");
        var twoMonthsAgo = now.AddMonths(-2).ToString("yyyy-MM");
        var threeMonthsAgo = now.AddMonths(-3).ToString("yyyy-MM");

        var batch = Interlocked.Increment(ref _seedCounter);
        var existingCount = await _context.Customers.CountAsync(ct);

        // ── Pick a unique customer profile ───────────────────────────────────
        var profiles = GetCustomerProfiles();
        var profile = profiles[(existingCount + batch - 1) % profiles.Length];
        var suffix = existingCount > profiles.Length ? $".{batch}" : "";

        var customer = new Customer
        {
            FullName = profile.Name,
            Email = $"{profile.EmailPrefix}{suffix}@example.com",
            PhoneNumber = profile.Phone
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Seed batch #{Batch}: Created customer '{Name}'", batch, customer.FullName);

        // ── Create subscriptions (4-6 random) ──────────────────────────────────
        var allSubTypes = new (SubscriptionType Type, string Provider, string Prefix)[]
        {
            (SubscriptionType.Electricity, profile.Providers[0], "EL"),
            (SubscriptionType.Water, profile.Providers[1], "WA"),
            (SubscriptionType.Internet, profile.Providers[2], "INT"),
            (SubscriptionType.NaturalGas, profile.Providers[3], "NG"),
            (SubscriptionType.GSM, profile.Providers[4], "GSM"),
            (SubscriptionType.Insurance, $"{profile.Providers[0]} Sigorta", "INS"),
        };

        // Shuffle and take 4-6
        var subCount = Rng.Next(4, 7); // 4, 5, or 6
        var selectedSubs = allSubTypes.OrderBy(_ => Rng.Next()).Take(subCount).ToArray();

        // Set CreatedAt to 4+ months before warped "now" so all billing periods (month-3 to month+1) are valid
        var subCreatedAt = now.AddMonths(-4);

        var subscriptions = selectedSubs
            .Select(s => MakeSub(customer.Id, s.Type, s.Provider, GenerateSubNo(s.Prefix), RandomDay(), subCreatedAt))
            .ToList();

        // Randomly make 1 subscription passive (cancelled)
        var passiveIndex = Rng.Next(subscriptions.Count);
        subscriptions[passiveIndex].Status = SubscriptionStatus.Passive;

        _context.Subscriptions.AddRange(subscriptions);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Seed batch #{Batch}: Created {Count} subscriptions", batch, subscriptions.Count);

        // ── Create payments (mix of paid/unpaid/failed) ──────────────────────
        var payments = new List<Payment>();
        var activeSubscriptions = subscriptions.Where(s => s.Status == SubscriptionStatus.Active).ToList();

        // Last 3 months: all active subscriptions were paid
        foreach (var sub in activeSubscriptions)
        {
            payments.Add(MakePayment(sub.Id, RandomAmount(sub.Type), threeMonthsAgo, PaymentStatus.Successful, now.AddMonths(-3)));
            payments.Add(MakePayment(sub.Id, RandomAmount(sub.Type), twoMonthsAgo, PaymentStatus.Successful, now.AddMonths(-2)));
            payments.Add(MakePayment(sub.Id, RandomAmount(sub.Type), lastMonth, PaymentStatus.Successful, now.AddMonths(-1)));
        }

        // Current month: pay ~half, leave rest unpaid for reminder testing
        var paidThisMonth = activeSubscriptions.Take(activeSubscriptions.Count / 2).ToList();
        var unpaidThisMonth = activeSubscriptions.Skip(activeSubscriptions.Count / 2).ToList();

        foreach (var sub in paidThisMonth)
        {
            payments.Add(MakePayment(sub.Id, RandomAmount(sub.Type), currentPeriod, PaymentStatus.Successful, now));
        }

        // Add a failed payment attempt on one unpaid subscription
        if (unpaidThisMonth.Any())
        {
            var failedSub = unpaidThisMonth.First();
            payments.Add(MakePayment(failedSub.Id, RandomAmount(failedSub.Type), currentPeriod, PaymentStatus.Failed, now));
        }

        _context.Payments.AddRange(payments);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Seed batch #{Batch}: Created {Count} payments", batch, payments.Count);

        // ── Build response ───────────────────────────────────────────────────
        var paidNames = paidThisMonth.Select(s => $"{s.Type} ({s.ServiceProvider})").ToArray();
        var unpaidNames = unpaidThisMonth.Select(s => $"{s.Type} ({s.ServiceProvider})").ToArray();
        var passiveName = $"{subscriptions[passiveIndex].Type} ({subscriptions[passiveIndex].ServiceProvider})";

        return new SeedResult(
            Success: true,
            Message: $"Seed batch #{batch} complete! Customer '{customer.FullName}' created with test data.",
            Data: new SeedData(
                Customer: new SeedCustomer(customer.Id, customer.FullName, customer.Email),
                Subscriptions: subscriptions.Select(s => new SeedSubscription(
                    s.Id, s.Type.ToString(), s.ServiceProvider, s.SubscriberNumber,
                    s.Status.ToString(), s.PaymentDayOfMonth)).ToArray(),
                CurrentPeriod: currentPeriod,
                PaidThisMonth: paidNames,
                UnpaidThisMonth: unpaidNames,
                PassiveSubscription: passiveName,
                TotalPaymentsCreated: payments.Count,
                TestInstructions: new[]
                {
                    $"🔍 Query debt:        GET /api/subscriptions/{unpaidThisMonth.FirstOrDefault()?.Id}/debt",
                    $"💰 Make payment:      POST /api/payments  (use amount from debt query, period: {currentPeriod})",
                    $"🔔 Check reminders:   GET /api/reminders?customerId={customer.Id}&daysAhead=7",
                    $"📋 Unpaid list:       GET /api/subscriptions/customer/{customer.Id}/unpaid",
                    $"📜 Payment history:   GET /api/payments/customer/{customer.Id}",
                    $"🔄 Seed more data:    POST /api/seed  (press again for another customer!)"
                }
            )
        );
    }

    public async Task<string> ClearAsync(CancellationToken ct = default)
    {
        await _context.Payments.ExecuteDeleteAsync(ct);
        await _context.Subscriptions.ExecuteDeleteAsync(ct);
        await _context.Customers.ExecuteDeleteAsync(ct);
        _logger.LogInformation("All seed data cleared.");
        return "All data cleared. Database is empty.";
    }

    // ── Helper methods ───────────────────────────────────────────────────────

    private static Subscription MakeSub(Guid customerId, SubscriptionType type, string provider, string number, int payDay, DateTime createdAt)
        => new()
        {
            CustomerId = customerId,
            Type = type,
            ServiceProvider = provider,
            SubscriberNumber = number,
            PaymentDayOfMonth = payDay,
            Status = SubscriptionStatus.Active,
            CreatedAt = createdAt
        };

    private static Payment MakePayment(Guid subId, decimal amount, string period, PaymentStatus status, DateTime around)
        => new()
        {
            SubscriptionId = subId,
            Amount = amount,
            Period = period,
            Status = status,
            PaymentDate = around.AddDays(-Rng.Next(0, 10)),
            TransactionReference = $"TXN-SEED-{Guid.NewGuid().ToString()[..8].ToUpper()}"
        };

    private static decimal RandomAmount(SubscriptionType type) => type switch
    {
        SubscriptionType.Electricity => Math.Round(80m + (decimal)Rng.NextDouble() * 270m, 2),
        SubscriptionType.Water => Math.Round(30m + (decimal)Rng.NextDouble() * 120m, 2),
        SubscriptionType.Internet => Math.Round(100m + (decimal)Rng.NextDouble() * 200m, 2),
        SubscriptionType.GSM => Math.Round(50m + (decimal)Rng.NextDouble() * 150m, 2),
        SubscriptionType.NaturalGas => Math.Round(60m + (decimal)Rng.NextDouble() * 340m, 2),
        SubscriptionType.Insurance => Math.Round(200m + (decimal)Rng.NextDouble() * 600m, 2),
        _ => Math.Round(50m + (decimal)Rng.NextDouble() * 200m, 2)
    };

    private static int RandomDay() => Rng.Next(1, 29);
    private static string GenerateSubNo(string prefix) => $"{prefix}-{Rng.Next(100000, 999999)}";

    private static CustomerProfile[] GetCustomerProfiles() => new[]
    {
        new CustomerProfile("Ahmet Yılmaz", "ahmet.yilmaz", "+905551112233",
            new[] { "BEDAŞ", "İSKİ", "Türk Telekom", "İGDAŞ", "Vodafone" }),
        new CustomerProfile("Elif Demir", "elif.demir", "+905559998877",
            new[] { "Enerjisa", "ASKİ", "Superonline", "EnerjiSA Doğalgaz", "Turkcell" }),
        new CustomerProfile("Mehmet Kaya", "mehmet.kaya", "+905554443322",
            new[] { "CK Enerji", "İSKİ", "TurkNet", "İGDAŞ", "Türk Telekom Mobil" }),
        new CustomerProfile("Zeynep Arslan", "zeynep.arslan", "+905557776655",
            new[] { "BEDAŞ", "ASKİ", "Millenicom", "Başkentgaz", "Vodafone" }),
        new CustomerProfile("Can Özturk", "can.ozturk", "+905553334411",
            new[] { "Toroslar EDAŞ", "MESKİ", "Superonline", "İGDAŞ", "Turkcell" }),
        new CustomerProfile("Ayşe Çelik", "ayse.celik", "+905552221100",
            new[] { "Dicle EDAŞ", "DİSKİ", "Türk Telekom", "Aksa Doğalgaz", "Türk Telekom Mobil" }),
        new CustomerProfile("Burak Şahin", "burak.sahin", "+905558887766",
            new[] { "GDZ Elektrik", "İZSU", "TurkNet", "İzgaz", "Vodafone" }),
        new CustomerProfile("Deniz Yıldız", "deniz.yildiz", "+905556665544",
            new[] { "Uludağ EDAŞ", "BUSKİ", "Millenicom", "Bursagaz", "Turkcell" }),
    };

    private record CustomerProfile(string Name, string EmailPrefix, string Phone, string[] Providers);
}

// ── Response DTOs ────────────────────────────────────────────────────────────
public record SeedResult(bool Success, string Message, SeedData? Data = null);
public record SeedData(
    SeedCustomer Customer,
    SeedSubscription[] Subscriptions,
    string CurrentPeriod,
    string[] PaidThisMonth,
    string[] UnpaidThisMonth,
    string PassiveSubscription,
    int TotalPaymentsCreated,
    string[] TestInstructions
);
public record SeedCustomer(Guid Id, string Name, string Email);
public record SeedSubscription(Guid Id, string Type, string ServiceProvider, string SubscriberNumber, string Status, int PaymentDayOfMonth);


