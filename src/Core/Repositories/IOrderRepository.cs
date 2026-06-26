using MultiTenantShop.Core.Entities;

namespace MultiTenantShop.Core.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string tenantId, string orderId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByCustomerAsync(string tenantId, string customerId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByStatusAsync(string tenantId, string status, CancellationToken ct = default);
    Task CreateAsync(Tenant tenant, Order order, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, Order order, CancellationToken ct = default);
}