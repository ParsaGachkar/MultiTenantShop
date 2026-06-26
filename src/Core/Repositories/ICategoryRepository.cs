using MultiTenantShop.Core.Entities;

namespace MultiTenantShop.Core.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(string tenantId, string categoryId, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetAllAsync(string tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(string tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetChildrenAsync(string tenantId, string parentId, CancellationToken ct = default);
    Task CreateAsync(Tenant tenant, Category category, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, Category category, CancellationToken ct = default);
    Task DeleteAsync(Tenant tenant, string categoryId, CancellationToken ct = default);
}