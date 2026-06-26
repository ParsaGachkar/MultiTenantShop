using MultiTenantShop.Core.Entities;

namespace MultiTenantShop.Core.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Tenant?> GetByIdAsync(string tenantId, CancellationToken ct = default);
    Task CreateAsync(Tenant tenant, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);
    Task DeleteAsync(string tenantId, CancellationToken ct = default);
}