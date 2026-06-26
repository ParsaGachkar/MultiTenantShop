using MultiTenantShop.Core.Entities;

namespace MultiTenantShop.Core.Repositories;

public interface IInventoryMovementRepository
{
    Task<InventoryMovement?> GetByIdAsync(string tenantId, string movementId, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryMovement>> GetByProductAsync(string tenantId, string productId, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryMovement>> GetByReasonAsync(string tenantId, string reason, CancellationToken ct = default);
    Task CreateAsync(Tenant tenant, InventoryMovement movement, CancellationToken ct = default);
}