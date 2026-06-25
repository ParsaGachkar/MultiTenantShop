namespace MultiTenantShop.Core.ValueObjects;

public class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative", nameof(factor));
        return new Money(Amount * factor, Currency);
    }

    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);

        if (other.Amount > Amount)
            throw new InvalidOperationException("Cannot subtract a larger amount");

        return new Money(Amount - other.Amount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Currency mismatch: {Currency} vs {other.Currency}");
    }

    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount &&
               string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) =>
        obj is Money other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(Amount, Currency.ToUpperInvariant());

    public override string ToString() =>
        $"{Amount:N2} {Currency}";

    public static bool operator ==(Money? left, Money? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Money? left, Money? right) =>
        !(left == right);
}
