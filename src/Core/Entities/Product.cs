using MultiTenantShop.Core.Enums;
using MultiTenantShop.Core.Interfaces;
using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Core.Entities;

public class Product : ITenantScoped
{
    private readonly List<Money> _prices = [];
    private readonly List<InventoryMovement> _movements = [];

    public string ProductId { get; private set; }
    public string TenantId { get; private set; }
    public string CategoryId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public string SKU { get; private set; }
    public int StockQuantity { get; private set; }
    public int LowStockThreshold { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public List<ProductVariant> Variants { get; private set; } = [];

    public IReadOnlyList<Money> Prices => _prices.AsReadOnly();
    public IReadOnlyList<InventoryMovement> Movements => _movements.AsReadOnly();

    public Product(
        string productId,
        string tenantId,
        string categoryId,
        string name,
        string description,
        string sku,
        Money price,
        int stockQuantity = 0,
        int lowStockThreshold = 5)
    {
        ProductId = productId;
        TenantId = tenantId;
        CategoryId = categoryId;
        Name = name;
        Description = description;
        SKU = sku;
        StockQuantity = stockQuantity;
        LowStockThreshold = lowStockThreshold;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        _prices.Add(price);
    }

    public void AddPrice(Money price)
    {
        var existing = _prices.Find(p =>
            string.Equals(p.Currency, price.Currency, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
            _prices.Remove(existing);

        _prices.Add(price);
    }

    public Money GetPrice(string currency)
    {
        var price = _prices.Find(p =>
            string.Equals(p.Currency, currency, StringComparison.OrdinalIgnoreCase));
        return price ?? throw new KeyNotFoundException(
            $"No price found for currency '{currency}'");
    }

    public bool IsLowStock() => StockQuantity > 0 && StockQuantity <= LowStockThreshold;

    public bool IsOutOfStock() => StockQuantity <= 0;

    public void AdjustStock(int quantity, MovementReason reason, string? referenceId = null, string? note = null)
    {
        if (quantity == 0)
            throw new ArgumentException("Quantity must be non-zero", nameof(quantity));

        var newStock = StockQuantity + quantity;
        if (newStock < 0)
            throw new InvalidOperationException(
                $"Insufficient stock. Current: {StockQuantity}, requested adjustment: {quantity}");

        StockQuantity = newStock;

        var movement = new InventoryMovement(
            Ulid.NewUlid().ToString(),
            ProductId,
            null,
            quantity,
            reason,
            referenceId,
            note);

        _movements.Add(movement);
    }

    public void Archive()
    {
        IsActive = false;
    }

    public void Restore()
    {
        IsActive = true;
    }
}
