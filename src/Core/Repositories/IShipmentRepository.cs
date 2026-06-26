using MultiTenantShop.Core.Entities;

namespace MultiTenantShop.Core.Repositories;

public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(string tenantId, string shipmentId, CancellationToken ct = default);
    Task<IReadOnlyList<Shipment>> GetByOrderAsync(string tenantId, string orderId, CancellationToken ct = default);
    Task CreateAsync(Tenant tenant, Shipment shipment, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, Shipment shipment, CancellationToken ct = default);
}