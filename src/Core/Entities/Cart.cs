using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Core.Entities;

public class Cart
{
    private readonly List<CartItem> _items = [];

    public string CartId { get; private set; }
    public string TenantId { get; private set; }
    public string CustomerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    public Cart(string cartId, string tenantId, string customerId)
    {
        CartId = cartId;
        TenantId = tenantId;
        CustomerId = customerId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void AddProduct(Product product, int quantity, string? variantId = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        var existing = _items.Find(i =>
            i.ProductId == product.ProductId && i.VariantId == variantId);

        if (existing is not null)
        {
            _items.Remove(existing);
            _items.Add(new CartItem(
                existing.CartItemId, existing.CartId, existing.ProductId,
                existing.VariantId, existing.Quantity + quantity, existing.UnitPrice));
        }
        else
        {
            var price = product.GetPrice("IRR"); // TODO: use tenant default currency
            var item = new CartItem(
                Ulid.NewUlid().ToString(),
                CartId,
                product.ProductId,
                variantId,
                quantity,
                price.Amount);
            _items.Add(item);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveProduct(string productId, string? variantId = null)
    {
        var item = _items.Find(i =>
            i.ProductId == productId && i.VariantId == variantId);

        if (item is null)
            throw new KeyNotFoundException("Product not found in cart");

        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    public Money CalculateTotal(string currency)
    {
        var total = _items.Sum(i => i.UnitPrice * i.Quantity);
        return new Money(total, currency);
    }
}
