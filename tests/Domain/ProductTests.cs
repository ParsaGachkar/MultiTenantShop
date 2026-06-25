using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Enums;
using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Tests.Domain;

public class ProductTests
{
    private readonly Money _basePrice = new(500_000, "IRR");

    private Product CreateProduct(int stock = 10)
    {
        return new Product(
            Ulid.NewUlid().ToString(),
            "tenant-1",
            "cat-1",
            "Test Product",
            "A test product",
            "TST-001",
            _basePrice,
            stock);
    }

    [Fact]
    public void Constructor_ShouldSetInitialPrice()
    {
        var product = CreateProduct();
        var price = product.GetPrice("IRR");
        Assert.Equal(500_000, price.Amount);
    }

    [Fact]
    public void AddPrice_ShouldReplace_ExistingCurrency()
    {
        var product = CreateProduct();
        var newPrice = new Money(600_000, "IRR");
        product.AddPrice(newPrice);
        Assert.Equal(600_000, product.GetPrice("IRR").Amount);
        Assert.Single(product.Prices);
    }

    [Fact]
    public void GetPrice_ShouldThrow_WhenCurrencyNotFound()
    {
        var product = CreateProduct();
        Assert.Throws<KeyNotFoundException>(() => product.GetPrice("USD"));
    }

    [Fact]
    public void IsLowStock_ShouldBeTrue_WhenAtThreshold()
    {
        var product = CreateProduct(stock: 5);
        Assert.True(product.IsLowStock());
    }

    [Fact]
    public void IsLowStock_ShouldBeFalse_WhenAboveThreshold()
    {
        var product = CreateProduct(stock: 20);
        Assert.False(product.IsLowStock());
    }

    [Fact]
    public void IsOutOfStock_ShouldBeTrue_WhenZero()
    {
        var product = CreateProduct(stock: 0);
        Assert.True(product.IsOutOfStock());
    }

    [Fact]
    public void AdjustStock_ShouldIncrease()
    {
        var product = CreateProduct(stock: 10);
        product.AdjustStock(5, MovementReason.Purchase, "PO-001");
        Assert.Equal(15, product.StockQuantity);
    }

    [Fact]
    public void AdjustStock_ShouldDecrease()
    {
        var product = CreateProduct(stock: 10);
        product.AdjustStock(-3, MovementReason.Sale, "ORD-001");
        Assert.Equal(7, product.StockQuantity);
    }

    [Fact]
    public void AdjustStock_ShouldThrow_WhenInsufficient()
    {
        var product = CreateProduct(stock: 2);
        Assert.Throws<InvalidOperationException>(() =>
            product.AdjustStock(-5, MovementReason.Sale));
    }

    [Fact]
    public void AdjustStock_ShouldRecordMovement()
    {
        var product = CreateProduct(stock: 10);
        product.AdjustStock(-2, MovementReason.Sale, "ORD-001");
        Assert.Single(product.Movements);
        Assert.Equal(-2, product.Movements[0].Quantity);
        Assert.Equal(MovementReason.Sale, product.Movements[0].Reason);
        Assert.Equal("ORD-001", product.Movements[0].ReferenceId);
    }

    [Fact]
    public void AdjustStock_ShouldThrow_WhenQuantityZero()
    {
        var product = CreateProduct();
        Assert.Throws<ArgumentException>(() =>
            product.AdjustStock(0, MovementReason.Adjustment));
    }

    [Fact]
    public void Archive_ShouldSetInactive()
    {
        var product = CreateProduct();
        product.Archive();
        Assert.False(product.IsActive);
    }

    [Fact]
    public void Restore_ShouldSetActive()
    {
        var product = CreateProduct();
        product.Archive();
        product.Restore();
        Assert.True(product.IsActive);
    }
}
