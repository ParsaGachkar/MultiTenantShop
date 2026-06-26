using MongoDB.Driver;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Repositories;
using MultiTenantShop.Infrastructure.Persistence;

namespace MultiTenantShop.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly TenantStore _tenantStore;

    public CartRepository(TenantStore tenantStore)
    {
        _tenantStore = tenantStore;
    }

    private IMongoCollection<Cart> GetCollection(Tenant tenant)
    {
        return _tenantStore.GetCollection<Cart>(tenant, "Carts");
    }

    private FilterDefinition<Cart> TenantFilter(string tenantId)
    {
        return Builders<Cart>.Filter.Eq(c => c.TenantId, tenantId);
    }

    public async Task<Cart?> GetByIdAsync(string tenantId, string cartId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Cart>.Filter.Eq(c => c.CartId, cartId)
            : Builders<Cart>.Filter.And(
                Builders<Cart>.Filter.Eq(c => c.CartId, cartId),
                TenantFilter(tenantId));

        return await collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<Cart?> GetByCustomerAsync(string tenantId, string customerId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Cart>.Filter.Eq(c => c.CustomerId, customerId)
            : Builders<Cart>.Filter.And(
                Builders<Cart>.Filter.Eq(c => c.CustomerId, customerId),
                TenantFilter(tenantId));

        return await collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task CreateAsync(Tenant tenant, Cart cart, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        await collection.InsertOneAsync(cart, cancellationToken: ct);
    }

    public async Task UpdateAsync(Tenant tenant, Cart cart, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Cart>.Filter.Eq(c => c.CartId, cart.CartId)
            : Builders<Cart>.Filter.And(
                Builders<Cart>.Filter.Eq(c => c.CartId, cart.CartId),
                TenantFilter(tenant.TenantId));

        await collection.ReplaceOneAsync(filter, cart, cancellationToken: ct);
    }

    public async Task DeleteAsync(Tenant tenant, string cartId, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Cart>.Filter.Eq(c => c.CartId, cartId)
            : Builders<Cart>.Filter.And(
                Builders<Cart>.Filter.Eq(c => c.CartId, cartId),
                TenantFilter(tenant.TenantId));

        await collection.DeleteOneAsync(filter, ct);
    }
}