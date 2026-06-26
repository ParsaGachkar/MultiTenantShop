using MongoDB.Driver;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Repositories;
using MultiTenantShop.Infrastructure.Persistence;

namespace MultiTenantShop.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly TenantStore _tenantStore;

    public CategoryRepository(TenantStore tenantStore)
    {
        _tenantStore = tenantStore;
    }

    private IMongoCollection<Category> GetCollection(Tenant tenant)
    {
        return _tenantStore.GetCollection<Category>(tenant, "Categories");
    }

    private FilterDefinition<Category> TenantFilter(string tenantId)
    {
        return Builders<Category>.Filter.Eq(c => c.TenantId, tenantId);
    }

    public async Task<Category?> GetByIdAsync(string tenantId, string categoryId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Category>.Filter.Eq(c => c.CategoryId, categoryId)
            : Builders<Category>.Filter.And(
                Builders<Category>.Filter.Eq(c => c.CategoryId, categoryId),
                TenantFilter(tenantId));

        return await collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(string tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<Category>();

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? FilterDefinition<Category>.Empty
            : TenantFilter(tenantId);

        return await collection.Find(filter).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(string tenantId, CancellationToken ct = default)
    {
        return await GetAllAsync(tenantId, ct);
    }

    public async Task<IReadOnlyList<Category>> GetChildrenAsync(string tenantId, string parentId, CancellationToken ct = default)
    {
        return Array.Empty<Category>();
    }

    public async Task CreateAsync(Tenant tenant, Category category, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        await collection.InsertOneAsync(category, cancellationToken: ct);
    }

    public async Task UpdateAsync(Tenant tenant, Category category, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Category>.Filter.Eq(c => c.CategoryId, category.CategoryId)
            : Builders<Category>.Filter.And(
                Builders<Category>.Filter.Eq(c => c.CategoryId, category.CategoryId),
                TenantFilter(tenant.TenantId));

        await collection.ReplaceOneAsync(filter, category, cancellationToken: ct);
    }

    public async Task DeleteAsync(Tenant tenant, string categoryId, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Category>.Filter.Eq(c => c.CategoryId, categoryId)
            : Builders<Category>.Filter.And(
                Builders<Category>.Filter.Eq(c => c.CategoryId, categoryId),
                TenantFilter(tenant.TenantId));

        await collection.DeleteOneAsync(filter, ct);
    }
}