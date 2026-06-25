using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Tests.Domain;

public class AddressTests
{
    [Fact]
    public void Constructor_ShouldCreate_WhenValid()
    {
        var addr = new Address("Tehran", "Tehran", "1234567891", "Enghelab St");
        Assert.Equal("Tehran", addr.Province);
        Assert.Equal("Tehran", addr.City);
        Assert.Equal("1234567891", addr.PostalCode);
        Assert.Equal("Enghelab St", addr.Street);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenProvinceMissing()
    {
        Assert.Throws<ArgumentException>(() =>
            new Address("", "Tehran", "12345", "St"));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCityMissing()
    {
        Assert.Throws<ArgumentException>(() =>
            new Address("Tehran", "", "12345", "St"));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenStreetMissing()
    {
        Assert.Throws<ArgumentException>(() =>
            new Address("Tehran", "Tehran", "12345", ""));
    }

    [Fact]
    public void Format_English_ShouldReturnExpected()
    {
        var addr = new Address("Tehran", "Tehran", "12345", "Enghelab St");
        var result = addr.Format(false);
        Assert.Equal("Enghelab St, Tehran, Tehran 12345", result);
    }

    [Fact]
    public void Format_Persian_ShouldReturnExpected()
    {
        var addr = new Address("تهران", "تهران", "۱۲۳۴۵", "خیابان انقلاب");
        var result = addr.Format(true);
        Assert.Equal("خیابان انقلاب, تهران, تهران — ۱۲۳۴۵", result);
    }

    [Fact]
    public void Equals_ShouldBeTrue_WhenSame()
    {
        var a = new Address("Tehran", "Tehran", "12345", "Enghelab");
        var b = new Address("Tehran", "Tehran", "12345", "Enghelab");
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_ShouldBeFalse_WhenDifferent()
    {
        var a = new Address("Tehran", "Tehran", "12345", "Enghelab");
        var b = new Address("Tehran", "Tehran", "12345", "Valiasr");
        Assert.NotEqual(a, b);
    }
}
