using MultiTenantShop.Core.Enums;
using MultiTenantShop.Core.Interfaces;
using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Core.Entities;

public class Order : ITenantScoped
{
    private readonly List<OrderItem> _items = [];

    public string OrderId { get; private set; }
    public string TenantId { get; private set; }
    public string CustomerId { get; private set; }
    public string OrderNumber { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal ShippingTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal GrandTotal { get; private set; }
    public string Currency { get; private set; }
    public string? CouponId { get; private set; }
    public string? ShippingAddress { get; private set; }
    public string? BillingAddress { get; private set; }
    public string? CultureName { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public Order(
        string orderId,
        string tenantId,
        string customerId,
        string orderNumber,
        string currency,
        string? cultureName = null)
    {
        OrderId = orderId;
        TenantId = tenantId;
        CustomerId = customerId;
        OrderNumber = orderNumber;
        Currency = currency;
        Status = OrderStatus.Pending;
        CultureName = cultureName;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void AddItem(
        string productId,
        string? variantId,
        string productName,
        string sku,
        int quantity,
        Money unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Can only add items to a pending order");
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        unitPrice = EnsureCurrency(unitPrice);

        var item = new OrderItem(
            Ulid.NewUlid().ToString(),
            OrderId,
            productId,
            variantId,
            productName,
            sku,
            quantity,
            unitPrice.Amount);

        _items.Add(item);
        RecalculateTotals();
    }

    public void ApplyCoupon(Coupon coupon)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Can only apply coupon to a pending order");

        var result = coupon.CanApply(this);
        if (!result.IsValid)
            throw new InvalidOperationException(result.Error);

        CouponId = coupon.CouponId;

        var discount = coupon.CalculateDiscount(this);
        DiscountAmount = discount.Amount;
        RecalculateTotals();
    }

    public void RemoveCoupon()
    {
        CouponId = null;
        DiscountAmount = 0;
        RecalculateTotals();
    }

    public void SetShippingAddress(string address)
    {
        ShippingAddress = address;
    }

    public void SetBillingAddress(string address)
    {
        BillingAddress = address;
    }

    public void Submit()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Order is not in pending state");
        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot submit an empty order");

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Refunded)
            throw new InvalidOperationException(
                $"Cannot cancel order in '{Status}' status");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public Money CalculateTotal()
    {
        return new Money(GrandTotal, Currency);
    }

    private void RecalculateTotals()
    {
        SubTotal = _items.Sum(i => i.TotalPrice);
        GrandTotal = SubTotal + TaxTotal + ShippingTotal - DiscountAmount;
        if (GrandTotal < 0) GrandTotal = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    private Money EnsureCurrency(Money money)
    {
        if (!string.Equals(money.Currency, Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Currency mismatch: expected {Currency}, got {money.Currency}");
        return money;
    }
}
