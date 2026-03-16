using Clasess.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Clasess.Tests;

public sealed class SubscriptionServiceTests
{
    private static SubDbContext CreateDbContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<SubDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new SubDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public void AddSubscription_Null_Throws()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var db = CreateDbContext(connection);

        var service = new SubscriptionService(db);

        Assert.Throws<ArgumentNullException>(() => service.AddSubscription(null!));
    }

    [Fact]
    public void AddSubscription_PersistsEntity()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var db = CreateDbContext(connection);

        var service = new SubscriptionService(db);

        var subscription = new Subscription
        {
            ServiceName = "Netflix",
            Amount = 1000m,
            Currency = "ARS",
            BillingCycle = "Mensual",
            NextBillingDate = new DateTime(2026, 03, 01),
            Status = "Activa",
            AutoPayment = false,
            Recordatorio = 0,
        };

        service.AddSubscription(subscription);

        var fromDb = db.Subscriptions.Single();
        Assert.Equal("Netflix", fromDb.ServiceName);
        Assert.Equal(1000m, fromDb.Amount);
    }

    [Fact]
    public void DeactivateSubscription_Null_Throws()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var db = CreateDbContext(connection);

        var service = new SubscriptionService(db);

        Assert.Throws<ArgumentNullException>(() => service.DeactivateSubscription(null));
    }

    [Fact]
    public void DeactivateSubscription_WhenMissing_ReturnsFalse()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var db = CreateDbContext(connection);

        var service = new SubscriptionService(db);

        var ok = service.DeactivateSubscription(12345);

        Assert.False(ok);
    }

    [Fact]
    public void DeactivateSubscription_SetsStatusToInactiva()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var db = CreateDbContext(connection);

        var subscription = new Subscription
        {
            ServiceName = "Spotify",
            Amount = 500m,
            Currency = "ARS",
            BillingCycle = "Mensual",
            NextBillingDate = new DateTime(2026, 03, 01),
            Status = "Activa",
        };

        db.Subscriptions.Add(subscription);
        db.SaveChanges();

        var service = new SubscriptionService(db);

        var ok = service.DeactivateSubscription(subscription.Id);

        Assert.True(ok);
        Assert.Equal("Inactiva", db.Subscriptions.Single().Status);
    }

    [Fact]
    public void RegisterPaymentAndCycle_AddsPaymentAndAdvancesDate_Monthly()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var db = CreateDbContext(connection);

        var originalNextBilling = new DateTime(2026, 03, 01);

        var subscription = new Subscription
        {
            ServiceName = "Disney+",
            Amount = 1200m,
            Currency = "ARS",
            BillingCycle = "Mensual",
            NextBillingDate = originalNextBilling,
            Status = "Activa",
        };

        db.Subscriptions.Add(subscription);
        db.SaveChanges();

        var service = new SubscriptionService(db);
        service.RegisterPaymentAndCycle(subscription.Id);

        var payment = db.Payments.Single();
        Assert.Equal(originalNextBilling, payment.Date);
        Assert.Equal(1200m, payment.Amount);

        var updatedSub = db.Subscriptions.Single();
        Assert.Equal(originalNextBilling.AddMonths(1), updatedSub.NextBillingDate);
    }

    [Fact]
    public void UndoLastPayment_RemovesPaymentAndRewindsNextBillingDate_Monthly()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var db = CreateDbContext(connection);

        var subscription = new Subscription
        {
            ServiceName = "Prime Video",
            Amount = 800m,
            Currency = "ARS",
            BillingCycle = "Mensual",
            NextBillingDate = new DateTime(2026, 04, 01),
            Status = "Activa",
        };

        subscription.Payments.Add(new Payment
        {
            Date = new DateTime(2026, 03, 01),
            Amount = 800m,
        });

        db.Subscriptions.Add(subscription);
        db.SaveChanges();

        var service = new SubscriptionService(db);
        service.UndoLastPayment(subscription.Id);

        Assert.Empty(db.Payments.ToList());
        Assert.Equal(new DateTime(2026, 03, 01), db.Subscriptions.Single().NextBillingDate);
    }
}

