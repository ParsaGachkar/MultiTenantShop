using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Enums;
using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Tests.Domain;

public class PaymentTests
{
    [Fact]
    public void Constructor_ShouldCreatePending()
    {
        var payment = new Payment("p1", "tenant-1", "ord-1", PaymentMethod.ZarinPal, new Money(500_000, "IRR"));
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void Succeed_ShouldTransition()
    {
        var payment = new Payment("p1", "tenant-1", "ord-1", PaymentMethod.ZarinPal, new Money(500_000, "IRR"));
        payment.Succeed("TXN-123");
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal("TXN-123", payment.TransactionId);
        Assert.NotNull(payment.PaidAt);
    }

    [Fact]
    public void Succeed_ShouldThrow_WhenNoTransactionId()
    {
        var payment = new Payment("p1", "tenant-1", "ord-1", PaymentMethod.ZarinPal, new Money(500_000, "IRR"));
        Assert.Throws<ArgumentException>(() => payment.Succeed(""));
    }

    [Fact]
    public void Fail_ShouldTransition()
    {
        var payment = new Payment("p1", "tenant-1", "ord-1", PaymentMethod.ZarinPal, new Money(500_000, "IRR"));
        payment.Fail();
        Assert.Equal(PaymentStatus.Failed, payment.Status);
    }

    [Fact]
    public void Refund_ShouldTransition()
    {
        var payment = new Payment("p1", "tenant-1", "ord-1", PaymentMethod.ZarinPal, new Money(500_000, "IRR"));
        payment.Succeed("TXN-123");
        payment.Refund();
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
    }

    [Fact]
    public void Refund_ShouldThrow_WhenNotCompleted()
    {
        var payment = new Payment("p1", "tenant-1", "ord-1", PaymentMethod.ZarinPal, new Money(500_000, "IRR"));
        Assert.Throws<InvalidOperationException>(() => payment.Refund());
    }

    [Fact]
    public void PartiallyRefund_ShouldTransition()
    {
        var payment = new Payment("p1", "tenant-1", "ord-1", PaymentMethod.ZarinPal, new Money(500_000, "IRR"));
        payment.Succeed("TXN-123");
        payment.PartiallyRefund();
        Assert.Equal(PaymentStatus.PartiallyRefunded, payment.Status);
    }
}