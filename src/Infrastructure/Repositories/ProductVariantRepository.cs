using MongoDB.Driver;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Repositories;
using MultiTenantShop.Infrastructure.Persistence;

namespace MultiTenantShop.Infrastructure.Repositories;

public class ProductVariantRepository : IProductVariantRepository
{
    private readonly TenantStore _tenantStore;

    public ProductVariantRepository(TenantStore tenantStore)
    {
        _tenantStore = tenantStore;
    }

    private IMongoCollection<ProductVariant> GetCollection(Tenant tenant)
    {
        return _tenantStore.GetCollection<ProductVariant>(tenant, "ProductVariants");
    }

    private FilterDefinition<ProductVariant> TenantFilter(string tenantId)
    {
        return Builders<ProductVariant>.Filter.Eq(v => v.TenantId, tenantId);
    }

    public async Task<ProductVariant?> GetByIdAsync(string tenantId, string variantId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<ProductVariant>.Filter.Eq(v => v.VariantId, variantId)
            : Builders<ProductVariant>.Filter.And(
                Builders<ProductVariant>.Filter.Eq(v => v.VariantId, variantId),
                TenantFilter(tenantId));

        return await collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<ProductVariant>> GetByProductAsync(string tenantId, string productId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<ProductVariant>();

        var collection = GetCollection(tenant);
        var productFilter = Builders<ProductVariant>.Filter.Eq(v => v.ProductId, productId);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? productFilter
            : Builders<ProductVariant>.Filter.And(productFilter, TenantFilter(tenantId));

        return await collection.Find(filter).ToListAsync(ct);
    }

    public async Task CreateAsync(Tenant tenant, ProductVariant variant, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        await collection.InsertOneAsync(variant, cancellationToken: ct);
    }

    public async Task UpdateAsync(Tenant tenant, ProductVariant variant, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<ProductVariant>.Filter.Eq(v => v.VariantId, variant.VariantId)
            : Builders<ProductVariant>.Filter.And(
                Builders<ProductVariant>.Filter.Eq(v => v.VariantId, variant.VariantId),
                TenantFilter(tenant.TenantId));

        await collection.ReplaceOneAsync(filter, variant, cancellationToken: ct);
    }

    public async Task DeleteAsync(Tenant tenant, string variantId, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<ProductVariant>.Filter.Eq(v => v.VariantId, variantId)
            : Builders<ProductVariant>.Filter.And(
                Builders<ProductVariant>.Filter.Eq(v => v.VariantId, variantId),
                TenantFilter(tenant.TenantId));

        await collection.DeleteOneAsync(filter, ct);
    }
}