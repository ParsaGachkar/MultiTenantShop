using MultiTenantShop.Core.Entities;

namespace MultiTenantShop.Core.Repositories;

public interface IProductVariantRepository
{
    Task<ProductVariant?> GetByIdAsync(string tenantId, string variantId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductVariant>> GetByProductAsync(string tenantId, string productId, CancellationToken ct = default);
    Task CreateAsync(Tenant tenant, ProductVariant variant, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, ProductVariant variant, CancellationToken ct = default);
    Task DeleteAsync(Tenant tenant, string variantId, CancellationToken ct = default);
}