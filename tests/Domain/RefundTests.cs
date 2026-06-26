using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Enums;
using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Tests.Domain;

public class RefundTests
{
    [Fact]
    public void Constructor_ShouldCreatePending()
    {
        var refund = new Refund("r1", "tenant-1", "ord-1", "pay-1", new Money(100_000, "IRR"));
        Assert.Equal(RefundStatus.Pending, refund.Status);
        Assert.True(refund.Restock);
    }

    [Fact]
    public void Approve_ShouldTransition()
    {
        var refund = new Refund("r1", "tenant-1", "ord-1", "pay-1", new Money(100_000, "IRR"));
        refund.Approve();
        Assert.Equal(RefundStatus.Approved, refund.Status);
    }

    [Fact]
    public void Approve_ShouldThrow_WhenNotPending()
    {
        var refund = new Refund("r1", "tenant-1", "ord-1", "pay-1", new Money(100_000, "IRR"));
        refund.Approve();
        Assert.Throws<InvalidOperationException>(() => refund.Approve());
    }

    [Fact]
    public void Reject_ShouldTransition()
    {
        var refund = new Refund("r1", "tenant-1", "ord-1", "pay-1", new Money(100_000, "IRR"));
        refund.Reject("Duplicate request");
        Assert.Equal(RefundStatus.Rejected, refund.Status);
    }

    [Fact]
    public void Complete_ShouldWork_WhenApproved()
    {
        var refund = new Refund("r1", "tenant-1", "ord-1", "pay-1", new Money(100_000, "IRR"));
        refund.Approve();
        refund.Complete();
        Assert.Equal(RefundStatus.Completed, refund.Status);
    }

    [Fact]
    public void Complete_ShouldThrow_WhenNotApproved()
    {
        var refund = new Refund("r1", "tenant-1", "ord-1", "pay-1", new Money(100_000, "IRR"));
        Assert.Throws<InvalidOperationException>(() => refund.Complete());
    }
}
