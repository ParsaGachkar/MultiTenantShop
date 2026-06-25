namespace MultiTenantShop.Core.Entities;

public class CartItem
{
    public string CartItemId { get; private set; }
    public string CartId { get; private set; }
    public string ProductId { get; private set; }
    public string? VariantId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public CartItem(
        string cartItemId,
        string cartId,
        string productId,
        string? variantId,
        int quantity,
        decimal unitPrice)
    {
        CartItemId = cartItemId;
        CartId = cartId;
        ProductId = productId;
        VariantId = variantId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
