using MongoDB.Driver;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Repositories;
using MultiTenantShop.Infrastructure.Persistence;

namespace MultiTenantShop.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly TenantStore _tenantStore;

    public ProductRepository(TenantStore tenantStore)
    {
        _tenantStore = tenantStore;
    }

    private IMongoCollection<Product> GetCollection(Tenant tenant)
    {
        return _tenantStore.GetCollection<Product>(tenant, "Products");
    }

    private FilterDefinition<Product> TenantFilter(string tenantId)
    {
        return Builders<Product>.Filter.Eq(p => p.TenantId, tenantId);
    }

    public async Task<Product?> GetByIdAsync(string tenantId, string productId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Product>.Filter.Eq(p => p.ProductId, productId)
            : Builders<Product>.Filter.And(
                Builders<Product>.Filter.Eq(p => p.ProductId, productId),
                TenantFilter(tenantId));

        return await collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(string tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<Product>();

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? FilterDefinition<Product>.Empty
            : TenantFilter(tenantId);

        return await collection.Find(filter).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(string tenantId, string categoryId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<Product>();

        var collection = GetCollection(tenant);
        var categoryFilter = Builders<Product>.Filter.Eq(p => p.CategoryId, categoryId);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? categoryFilter
            : Builders<Product>.Filter.And(categoryFilter, TenantFilter(tenantId));

        return await collection.Find(filter).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetActiveAsync(string tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<Product>();

        var collection = GetCollection(tenant);
        var activeFilter = Builders<Product>.Filter.Eq(p => p.IsActive, true);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? activeFilter
            : Builders<Product>.Filter.And(activeFilter, TenantFilter(tenantId));

        return await collection.Find(filter).ToListAsync(ct);
    }

    public async Task CreateAsync(Tenant tenant, Product product, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        await collection.InsertOneAsync(product, cancellationToken: ct);
    }

    public async Task UpdateAsync(Tenant tenant, Product product, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Product>.Filter.Eq(p => p.ProductId, product.ProductId)
            : Builders<Product>.Filter.And(
                Builders<Product>.Filter.Eq(p => p.ProductId, product.ProductId),
                TenantFilter(tenant.TenantId));

        await collection.ReplaceOneAsync(filter, product, cancellationToken: ct);
    }

    public async Task DeleteAsync(Tenant tenant, string productId, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Product>.Filter.Eq(p => p.ProductId, productId)
            : Builders<Product>.Filter.And(
                Builders<Product>.Filter.Eq(p => p.ProductId, productId),
                TenantFilter(tenant.TenantId));

        await collection.DeleteOneAsync(filter, ct);
    }
}