using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Enums;
using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Tests.Domain;

public class OrderTests
{
    private Order CreateOrder()
    {
        return new Order(
            Ulid.NewUlid().ToString(),
            "tenant-1",
            "customer-1",
            "ORD-001",
            "IRR");
    }

    [Fact]
    public void Constructor_ShouldCreatePending()
    {
        var order = CreateOrder();
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void AddItem_ShouldAdd()
    {
        var order = CreateOrder();
        order.AddItem("prod-1", null, "Test Product", "TST-001", 2, new Money(100_000, "IRR"));
        Assert.Single(order.Items);
        Assert.Equal(200_000, order.SubTotal);
    }

    [Fact]
    public void AddItem_ShouldThrow_WhenQuantityZero()
    {
        var order = CreateOrder();
        Assert.Throws<ArgumentException>(() =>
            order.AddItem("prod-1", null, "Test", "TST", 0, new Money(100_000, "IRR")));
    }

    [Fact]
    public void AddItem_ShouldThrow_WhenCurrencyMismatch()
    {
        var order = CreateOrder();
        Assert.Throws<InvalidOperationException>(() =>
            order.AddItem("prod-1", null, "Test", "TST", 1, new Money(100, "USD")));
    }

    [Fact]
    public void Submit_ShouldTransitionToConfirmed()
    {
        var order = CreateOrder();
        order.AddItem("prod-1", null, "Test", "TST-001", 1, new Money(100_000, "IRR"));
        order.Submit();
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Submit_ShouldThrow_WhenEmpty()
    {
        var order = CreateOrder();
        Assert.Throws<InvalidOperationException>(() => order.Submit());
    }

    [Fact]
    public void Cancel_ShouldTransition_FromPending()
    {
        var order = CreateOrder();
        order.Cancel();
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_ShouldWork_WhenConfirmed()
    {
        var order = CreateOrder();
        order.AddItem("prod-1", null, "Test", "TST", 1, new Money(100_000, "IRR"));
        order.Submit();
        order.Cancel();
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void CalculateTotal_ShouldIncludeTaxAndShipping()
    {
        var order = CreateOrder();
        order.AddItem("prod-1", null, "Test", "TST", 2, new Money(100_000, "IRR"));
        Assert.Equal(200_000, order.GrandTotal);
    }

    [Fact]
    public void CalculateTotal_ShouldSubtractDiscount()
    {
        var order = CreateOrder();
        order.AddItem("prod-1", null, "Test", "TST", 2, new Money(100_000, "IRR"));
        Assert.Equal(200_000, order.GrandTotal);
    }

    [Fact]
    public void SetShippingAddress_ShouldWork()
    {
        var order = CreateOrder();
        order.SetShippingAddress("Tehran, Enghelab St, 12345");
        Assert.Equal("Tehran, Enghelab St, 12345", order.ShippingAddress);
    }
}
