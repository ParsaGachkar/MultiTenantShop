using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Enums;
using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Tests.Domain;

public class CartTests
{
    private Cart CreateCart()
    {
        return new Cart(
            Ulid.NewUlid().ToString(),
            "tenant-1",
            "customer-1");
    }

    private Product CreateProduct(int stock = 10)
    {
        return new Product(
            Ulid.NewUlid().ToString(),
            "tenant-1",
            "cat-1",
            "Test Product",
            "Description",
            "TST-001",
            new Money(100_000, "IRR"),
            stock);
    }

    [Fact]
    public void Constructor_ShouldCreateEmpty()
    {
        var cart = CreateCart();
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void AddProduct_ShouldAdd_WhenNew()
    {
        var cart = CreateCart();
        var product = CreateProduct();
        cart.AddProduct(product, 2);
        Assert.Single(cart.Items);
        Assert.Equal(2, cart.Items[0].Quantity);
    }

    [Fact]
    public void AddProduct_ShouldIncrement_WhenExisting()
    {
        var cart = CreateCart();
        var product = CreateProduct();
        cart.AddProduct(product, 2);
        cart.AddProduct(product, 3);
        Assert.Single(cart.Items);
        Assert.Equal(5, cart.Items[0].Quantity);
    }

    [Fact]
    public void RemoveProduct_ShouldRemove()
    {
        var cart = CreateCart();
        var product = CreateProduct();
        cart.AddProduct(product, 1);
        cart.RemoveProduct(product.ProductId);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void RemoveProduct_ShouldThrow_WhenNotFound()
    {
        var cart = CreateCart();
        Assert.Throws<KeyNotFoundException>(() =>
            cart.RemoveProduct("nonexistent"));
    }

    [Fact]
    public void Clear_ShouldEmpty()
    {
        var cart = CreateCart();
        var product = CreateProduct();
        cart.AddProduct(product, 2);
        cart.Clear();
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void AddProduct_ShouldThrow_WhenQuantityNonPositive()
    {
        var cart = CreateCart();
        var product = CreateProduct();
        Assert.Throws<ArgumentException>(() => cart.AddProduct(product, 0));
    }
}
