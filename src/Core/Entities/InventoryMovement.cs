using MultiTenantShop.Core.Enums;
using MultiTenantShop.Core.Interfaces;

namespace MultiTenantShop.Core.Entities;

public class InventoryMovement : ITenantScoped
{
    public string MovementId { get; private set; }
    public string TenantId { get; private set; }
    public string ProductId { get; private set; }
    public string? VariantId { get; private set; }
    public int Quantity { get; private set; }
    public MovementReason Reason { get; private set; }
    public string? ReferenceId { get; private set; }
    public string? Note { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public InventoryMovement(
        string movementId,
        string tenantId,
        string productId,
        string? variantId,
        int quantity,
        MovementReason reason,
        string? referenceId = null,
        string? note = null)
    {
        if (quantity == 0)
            throw new ArgumentException("Quantity must be non-zero", nameof(quantity));

        MovementId = movementId;
        TenantId = tenantId;
        ProductId = productId;
        VariantId = variantId;
        Quantity = quantity;
        Reason = reason;
        ReferenceId = referenceId;
        Note = note;
        CreatedAt = DateTime.UtcNow;
    }
}
