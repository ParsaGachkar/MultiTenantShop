using MultiTenantShop.Core.Entities;

namespace MultiTenantShop.Core.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(string tenantId, string cartId, CancellationToken ct = default);
    Task<Cart?> GetByCustomerAsync(string tenantId, string customerId, CancellationToken ct = default);
    Task CreateAsync(Tenant tenant, Cart cart, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, Cart cart, CancellationToken ct = default);
    Task DeleteAsync(Tenant tenant, string cartId, CancellationToken ct = default);
}