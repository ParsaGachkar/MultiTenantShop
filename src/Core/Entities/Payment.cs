using MultiTenantShop.Core.Enums;
using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Core.Entities;

public class Payment
{
    public string PaymentId { get; private set; }
    public string OrderId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public string? TransactionId { get; private set; }
    public Money Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public Payment(
        string paymentId,
        string orderId,
        PaymentMethod method,
        Money amount)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Method = method;
        Amount = amount;
        Status = PaymentStatus.Pending;
    }

    public void Succeed(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID is required", nameof(transactionId));

        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        PaidAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = PaymentStatus.Failed;
    }

    public void Refund()
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Only completed payments can be refunded");

        Status = PaymentStatus.Refunded;
    }

    public void PartiallyRefund()
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Only completed payments can be partially refunded");

        Status = PaymentStatus.PartiallyRefunded;
    }
}
