using MultiTenantShop.Core.Enums;
using MultiTenantShop.Core.Interfaces;
using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Core.Entities;

public class CouponApplyResult
{
    public bool IsValid { get; }
    public string? Error { get; }

    public CouponApplyResult(bool isValid, string? error = null)
    {
        IsValid = isValid;
        Error = error;
    }

    public static CouponApplyResult Ok() => new(true);
    public static CouponApplyResult Fail(string error) => new(false, error);
}

public class Coupon : ITenantScoped
{
    public string CouponId { get; private set; }
    public string TenantId { get; private set; }
    public string Code { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public Money? MinOrderAmount { get; private set; }
    public int MaxUses { get; private set; }
    public int CurrentUses { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Coupon(
        string couponId,
        string tenantId,
        string code,
        DiscountType discountType,
        decimal discountValue,
        DateTime expiresAt,
        Money? minOrderAmount = null,
        int maxUses = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));
        if (discountValue <= 0)
            throw new ArgumentException("Discount value must be positive", nameof(discountValue));
        if (discountType == DiscountType.Percentage && discountValue > 100)
            throw new ArgumentException("Percentage discount cannot exceed 100", nameof(discountValue));

        CouponId = couponId;
        TenantId = tenantId;
        Code = code.Trim().ToUpperInvariant();
        DiscountType = discountType;
        DiscountValue = discountValue;
        MinOrderAmount = minOrderAmount;
        MaxUses = maxUses;
        ExpiresAt = expiresAt;
        IsActive = true;
        CurrentUses = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public CouponApplyResult CanApply(Order order)
    {
        if (!IsActive)
            return CouponApplyResult.Fail("Coupon is not active");

        if (DateTime.UtcNow > ExpiresAt)
            return CouponApplyResult.Fail("Coupon has expired");

        if (MaxUses > 0 && CurrentUses >= MaxUses)
            return CouponApplyResult.Fail("Coupon usage limit reached");

        if (MinOrderAmount is not null &&
            !string.Equals(order.Currency, MinOrderAmount.Currency, StringComparison.OrdinalIgnoreCase))
            return CouponApplyResult.Fail("Currency mismatch");

        if (MinOrderAmount is not null && order.SubTotal < MinOrderAmount.Amount)
            return CouponApplyResult.Fail(
                $"Minimum order amount is {MinOrderAmount}");

        return CouponApplyResult.Ok();
    }

    public Money CalculateDiscount(Order order)
    {
        var orderTotal = new Money(order.SubTotal, order.Currency);

        var discount = DiscountType switch
        {
            DiscountType.Percentage => orderTotal.Multiply(DiscountValue / 100m),
            DiscountType.Fixed => new Money(DiscountValue, order.Currency),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (discount.Amount > orderTotal.Amount)
            return orderTotal;

        return discount;
    }

    public void MarkUsed()
    {
        CurrentUses++;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
