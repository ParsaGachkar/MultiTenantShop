using MultiTenantShop.Core.Entities;

namespace MultiTenantShop.Core.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(string tenantId, string productId, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(string tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(string tenantId, string categoryId, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetActiveAsync(string tenantId, CancellationToken ct = default);
    Task CreateAsync(Tenant tenant, Product product, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, Product product, CancellationToken ct = default);
    Task DeleteAsync(Tenant tenant, string productId, CancellationToken ct = default);
}