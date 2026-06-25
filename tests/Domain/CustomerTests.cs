using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Tests.Domain;

public class CustomerTests
{
    [Fact]
    public void Constructor_ShouldCreate()
    {
        var customer = new Customer(
            Ulid.NewUlid().ToString(),
            "tenant-1",
            "test@example.com",
            "Ali",
            "Karimi");
        Assert.Equal("Ali", customer.FirstName);
        Assert.Equal("Karimi", customer.LastName);
        Assert.Equal("test@example.com", customer.Email);
    }

    [Fact]
    public void AddAddress_ShouldAdd()
    {
        var customer = new Customer("c1", "t1", "a@b.com", "Ali", "Karimi");
        var addr = new Address("Tehran", "Tehran", "12345", "Enghelab");
        customer.AddAddress(addr);
        Assert.Single(customer.Addresses);
    }

    [Fact]
    public void GetDefaultAddress_ShouldReturn_WhenExists()
    {
        var customer = new Customer("c1", "t1", "a@b.com", "Ali", "Karimi");
        var addr = new Address("Tehran", "Tehran", "12345", "Enghelab");
        customer.AddAddress(addr);
        var result = customer.GetDefaultAddress();
        Assert.Equal("Enghelab", result.Street);
    }

    [Fact]
    public void GetDefaultAddress_ShouldThrow_WhenNone()
    {
        var customer = new Customer("c1", "t1", "a@b.com", "Ali", "Karimi");
        Assert.Throws<InvalidOperationException>(() => customer.GetDefaultAddress());
    }
}
