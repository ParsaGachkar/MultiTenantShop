namespace MultiTenantShop.Core.Entities;

public class OrderItem
{
    public string OrderItemId { get; private set; }
    public string OrderId { get; private set; }
    public string ProductId { get; private set; }
    public string? VariantId { get; private set; }
    public string ProductName { get; private set; }
    public string SKU { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }

    public OrderItem(
        string orderItemId,
        string orderId,
        string productId,
        string? variantId,
        string productName,
        string sku,
        int quantity,
        decimal unitPrice)
    {
        OrderItemId = orderItemId;
        OrderId = orderId;
        ProductId = productId;
        VariantId = variantId;
        ProductName = productName;
        SKU = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = quantity * unitPrice;
    }
}
