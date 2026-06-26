using MultiTenantShop.Core.Entities;

namespace MultiTenantShop.Core.Repositories;

public interface IRefundRepository
{
    Task<Refund?> GetByIdAsync(string tenantId, string refundId, CancellationToken ct = default);
    Task<IReadOnlyList<Refund>> GetByOrderAsync(string tenantId, string orderId, CancellationToken ct = default);
    Task<IReadOnlyList<Refund>> GetByStatusAsync(string tenantId, string status, CancellationToken ct = default);
    Task CreateAsync(Tenant tenant, Refund refund, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, Refund refund, CancellationToken ct = default);
}