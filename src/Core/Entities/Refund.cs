using MultiTenantShop.Core.Enums;
using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Core.Entities;

public class Refund
{
    public string RefundId { get; private set; }
    public string OrderId { get; private set; }
    public string PaymentId { get; private set; }
    public string? Reason { get; private set; }
    public RefundStatus Status { get; private set; }
    public Money Amount { get; private set; }
    public bool Restock { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Refund(
        string refundId,
        string orderId,
        string paymentId,
        Money amount,
        string? reason = null,
        bool restock = true)
    {
        RefundId = refundId;
        OrderId = orderId;
        PaymentId = paymentId;
        Amount = amount;
        Reason = reason;
        Restock = restock;
        Status = RefundStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        if (Status != RefundStatus.Pending)
            throw new InvalidOperationException("Refund is not in pending state");

        Status = RefundStatus.Approved;
    }

    public void Reject(string? reason = null)
    {
        if (Status != RefundStatus.Pending)
            throw new InvalidOperationException("Refund is not in pending state");

        Status = RefundStatus.Rejected;
        Reason = reason;
    }

    public void Complete()
    {
        if (Status != RefundStatus.Approved)
            throw new InvalidOperationException("Refund must be approved first");

        Status = RefundStatus.Completed;
    }
}
