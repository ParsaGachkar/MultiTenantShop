using MongoDB.Driver;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Repositories;
using MultiTenantShop.Infrastructure.Persistence;

namespace MultiTenantShop.Infrastructure.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly TenantStore _tenantStore;

    public CouponRepository(TenantStore tenantStore)
    {
        _tenantStore = tenantStore;
    }

    private IMongoCollection<Coupon> GetCollection(Tenant tenant)
    {
        return _tenantStore.GetCollection<Coupon>(tenant, "Coupons");
    }

    private FilterDefinition<Coupon> TenantFilter(string tenantId)
    {
        return Builders<Coupon>.Filter.Eq(c => c.TenantId, tenantId);
    }

    public async Task<Coupon?> GetByIdAsync(string tenantId, string couponId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Coupon>.Filter.Eq(c => c.CouponId, couponId)
            : Builders<Coupon>.Filter.And(
                Builders<Coupon>.Filter.Eq(c => c.CouponId, couponId),
                TenantFilter(tenantId));

        return await collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<Coupon?> GetByCodeAsync(string tenantId, string code, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var collection = GetCollection(tenant);
        var codeFilter = Builders<Coupon>.Filter.Eq(c => c.Code, code.Trim().ToUpperInvariant());
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? codeFilter
            : Builders<Coupon>.Filter.And(codeFilter, TenantFilter(tenantId));

        return await collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Coupon>> GetAllAsync(string tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<Coupon>();

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? FilterDefinition<Coupon>.Empty
            : TenantFilter(tenantId);

        return await collection.Find(filter).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Coupon>> GetActiveAsync(string tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<Coupon>();

        var collection = GetCollection(tenant);
        var activeFilter = Builders<Coupon>.Filter.Eq(c => c.IsActive, true);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? activeFilter
            : Builders<Coupon>.Filter.And(activeFilter, TenantFilter(tenantId));

        return await collection.Find(filter).ToListAsync(ct);
    }

    public async Task CreateAsync(Tenant tenant, Coupon coupon, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        await collection.InsertOneAsync(coupon, cancellationToken: ct);
    }

    public async Task UpdateAsync(Tenant tenant, Coupon coupon, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Coupon>.Filter.Eq(c => c.CouponId, coupon.CouponId)
            : Builders<Coupon>.Filter.And(
                Builders<Coupon>.Filter.Eq(c => c.CouponId, coupon.CouponId),
                TenantFilter(tenant.TenantId));

        await collection.ReplaceOneAsync(filter, coupon, cancellationToken: ct);
    }

    public async Task DeleteAsync(Tenant tenant, string couponId, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Coupon>.Filter.Eq(c => c.CouponId, couponId)
            : Builders<Coupon>.Filter.And(
                Builders<Coupon>.Filter.Eq(c => c.CouponId, couponId),
                TenantFilter(tenant.TenantId));

        await collection.DeleteOneAsync(filter, ct);
    }
}
