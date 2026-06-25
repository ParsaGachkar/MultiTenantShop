using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Enums;
using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Tests.Domain;

public class CouponTests
{
    private Coupon CreatePercentageCoupon(decimal value = 10, decimal? minOrder = null)
    {
        return new Coupon(
            Ulid.NewUlid().ToString(),
            "tenant-1",
            "SAVE10",
            DiscountType.Percentage,
            value,
            DateTime.UtcNow.AddDays(30),
            minOrder is not null ? new Money(minOrder.Value, "IRR") : null,
            maxUses: 100);
    }

    private Coupon CreateFixedCoupon(decimal value = 50_000)
    {
        return new Coupon(
            Ulid.NewUlid().ToString(),
            "tenant-1",
            "FLAT50",
            DiscountType.Fixed,
            value,
            DateTime.UtcNow.AddDays(30),
            maxUses: 100);
    }

    private Order CreateOrderWithSubTotal(decimal subTotal)
    {
        var order = new Order(
            Ulid.NewUlid().ToString(),
            "tenant-1",
            "customer-1",
            "ORD-001",
            "IRR");
        order.AddItem("prod-1", null, "Test", "TST", 1, new Money(subTotal, "IRR"));
        return order;
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCodeEmpty()
    {
        Assert.Throws<ArgumentException>(() =>
            new Coupon("id", "t1", "", DiscountType.Percentage, 10,
                DateTime.UtcNow.AddDays(1)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenPercentageOver100()
    {
        Assert.Throws<ArgumentException>(() =>
            new Coupon("id", "t1", "BAD", DiscountType.Percentage, 150,
                DateTime.UtcNow.AddDays(1)));
    }

    [Fact]
    public void Constructor_ShouldUpperCode()
    {
        var coupon = new Coupon("id", "t1", "save10", DiscountType.Percentage, 10,
            DateTime.UtcNow.AddDays(1));
        Assert.Equal("SAVE10", coupon.Code);
    }

    [Fact]
    public void CanApply_ShouldSucceed_WhenValid()
    {
        var coupon = CreatePercentageCoupon();
        var order = CreateOrderWithSubTotal(500_000);
        var result = coupon.CanApply(order);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CanApply_ShouldFail_WhenInactive()
    {
        var coupon = CreatePercentageCoupon();
        coupon.Deactivate();
        var order = CreateOrderWithSubTotal(500_000);
        var result = coupon.CanApply(order);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CanApply_ShouldFail_WhenExpired()
    {
        var coupon = new Coupon("id", "t1", "EXPIRED", DiscountType.Percentage, 10,
            DateTime.UtcNow.AddDays(-1));
        var order = CreateOrderWithSubTotal(500_000);
        var result = coupon.CanApply(order);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CanApply_ShouldFail_WhenMaxUsesReached()
    {
        var coupon = CreatePercentageCoupon();
        for (int i = 0; i < 100; i++)
            coupon.MarkUsed();
        var order = CreateOrderWithSubTotal(500_000);
        var result = coupon.CanApply(order);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CanApply_ShouldFail_WhenBelowMinOrder()
    {
        var coupon = CreatePercentageCoupon(minOrder: 1_000_000);
        var order = CreateOrderWithSubTotal(500_000);
        var result = coupon.CanApply(order);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CalculateDiscount_ShouldReturnPercentage()
    {
        var coupon = CreatePercentageCoupon(10);
        var order = CreateOrderWithSubTotal(200_000);
        var discount = coupon.CalculateDiscount(order);
        Assert.Equal(20_000, discount.Amount);
    }

    [Fact]
    public void CalculateDiscount_ShouldReturnFixed()
    {
        var coupon = CreateFixedCoupon(50_000);
        var order = CreateOrderWithSubTotal(200_000);
        var discount = coupon.CalculateDiscount(order);
        Assert.Equal(50_000, discount.Amount);
    }

    [Fact]
    public void CalculateDiscount_ShouldCap_AtOrderTotal()
    {
        var coupon = CreateFixedCoupon(1_000_000);
        var order = CreateOrderWithSubTotal(200_000);
        var discount = coupon.CalculateDiscount(order);
        Assert.Equal(200_000, discount.Amount);
    }

    [Fact]
    public void ApplyCoupon_OnOrder_ShouldSetDiscount()
    {
        var coupon = CreatePercentageCoupon(10);
        var order = CreateOrderWithSubTotal(200_000);
        order.ApplyCoupon(coupon);
        Assert.Equal(20_000, order.DiscountAmount);
        Assert.Equal(180_000, order.GrandTotal);
    }

    [Fact]
    public void RemoveCoupon_ShouldClearDiscount()
    {
        var coupon = CreatePercentageCoupon(10);
        var order = CreateOrderWithSubTotal(200_000);
        order.ApplyCoupon(coupon);
        order.RemoveCoupon();
        Assert.Equal(0, order.DiscountAmount);
        Assert.Equal(200_000, order.GrandTotal);
    }

    [Fact]
    public void MarkUsed_ShouldIncrement()
    {
        var coupon = CreatePercentageCoupon();
        coupon.MarkUsed();
        coupon.MarkUsed();
        Assert.Equal(2, coupon.CurrentUses);
    }
}
