using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Core.Entities;

public class ProductVariant
{
    public string VariantId { get; private set; }
    public string ProductId { get; private set; }
    public string Name { get; private set; }
    public string Value { get; private set; }
    public Money PriceAdjustment { get; private set; }
    public int StockQuantity { get; private set; }

    public ProductVariant(
        string variantId,
        string productId,
        string name,
        string value,
        Money priceAdjustment,
        int stockQuantity = 0)
    {
        VariantId = variantId;
        ProductId = productId;
        Name = name;
        Value = value;
        PriceAdjustment = priceAdjustment;
        StockQuantity = stockQuantity;
    }
}
