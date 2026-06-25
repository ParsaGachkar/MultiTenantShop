using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Tests.Domain;

public class MoneyTests
{
    [Fact]
    public void Constructor_ShouldCreate_WhenValid()
    {
        var money = new Money(100, "IRR");
        Assert.Equal(100, money.Amount);
        Assert.Equal("IRR", money.Currency);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAmountNegative()
    {
        Assert.Throws<ArgumentException>(() => new Money(-1, "IRR"));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCurrencyEmpty()
    {
        Assert.Throws<ArgumentException>(() => new Money(100, ""));
    }

    [Fact]
    public void Constructor_ShouldNormalizeCurrency_ToUpper()
    {
        var money = new Money(100, "irr");
        Assert.Equal("IRR", money.Currency);
    }

    [Fact]
    public void Add_ShouldReturnSum_WhenSameCurrency()
    {
        var a = new Money(100, "IRR");
        var b = new Money(50, "IRR");
        var result = a.Add(b);
        Assert.Equal(150, result.Amount);
        Assert.Equal("IRR", result.Currency);
    }

    [Fact]
    public void Add_ShouldThrow_WhenCurrencyMismatch()
    {
        var a = new Money(100, "IRR");
        var b = new Money(50, "USD");
        Assert.Throws<InvalidOperationException>(() => a.Add(b));
    }

    [Fact]
    public void Multiply_ShouldReturnProduct()
    {
        var money = new Money(100, "IRR");
        var result = money.Multiply(2.5m);
        Assert.Equal(250, result.Amount);
        Assert.Equal("IRR", result.Currency);
    }

    [Fact]
    public void Multiply_ShouldThrow_WhenFactorNegative()
    {
        var money = new Money(100, "IRR");
        Assert.Throws<ArgumentException>(() => money.Multiply(-1));
    }

    [Fact]
    public void Subtract_ShouldReturnDifference()
    {
        var a = new Money(100, "IRR");
        var b = new Money(30, "IRR");
        var result = a.Subtract(b);
        Assert.Equal(70, result.Amount);
    }

    [Fact]
    public void Subtract_ShouldThrow_WhenResultNegative()
    {
        var a = new Money(30, "IRR");
        var b = new Money(100, "IRR");
        Assert.Throws<InvalidOperationException>(() => a.Subtract(b));
    }

    [Fact]
    public void Equals_ShouldBeTrue_WhenSameAmountAndCurrency()
    {
        var a = new Money(100, "IRR");
        var b = new Money(100, "IRR");
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_ShouldBeFalse_WhenDifferentAmount()
    {
        var a = new Money(100, "IRR");
        var b = new Money(200, "IRR");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Equals_ShouldBeFalse_WhenDifferentCurrency()
    {
        var a = new Money(100, "IRR");
        var b = new Money(100, "USD");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToString_ShouldFormat()
    {
        var money = new Money(1000.5m, "IRR");
        Assert.Contains("IRR", money.ToString());
        Assert.Contains("1,000", money.ToString());
    }

    [Fact]
    public void OperatorEquals_ShouldWork()
    {
        var a = new Money(100, "IRR");
        var b = new Money(100, "IRR");
        Assert.True(a == b);
        Assert.False(a != b);
    }
}
