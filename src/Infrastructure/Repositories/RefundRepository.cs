using MongoDB.Driver;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Repositories;
using MultiTenantShop.Infrastructure.Persistence;

namespace MultiTenantShop.Infrastructure.Repositories;

public class RefundRepository : IRefundRepository
{
    private readonly TenantStore _tenantStore;

    public RefundRepository(TenantStore tenantStore)
    {
        _tenantStore = tenantStore;
    }

    private IMongoCollection<Refund> GetCollection(Tenant tenant)
    {
        return _tenantStore.GetCollection<Refund>(tenant, "Refunds");
    }

    private FilterDefinition<Refund> TenantFilter(string tenantId)
    {
        return Builders<Refund>.Filter.Eq(r => r.TenantId, tenantId);
    }

    public async Task<Refund?> GetByIdAsync(string tenantId, string refundId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Refund>.Filter.Eq(r => r.RefundId, refundId)
            : Builders<Refund>.Filter.And(
                Builders<Refund>.Filter.Eq(r => r.RefundId, refundId),
                TenantFilter(tenantId));

        return await collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Refund>> GetByOrderAsync(string tenantId, string orderId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<Refund>();

        var collection = GetCollection(tenant);
        var orderFilter = Builders<Refund>.Filter.Eq(r => r.OrderId, orderId);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? orderFilter
            : Builders<Refund>.Filter.And(orderFilter, TenantFilter(tenantId));

        return await collection.Find(filter).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Refund>> GetByStatusAsync(string tenantId, string status, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<Refund>();

        var collection = GetCollection(tenant);
        var statusFilter = Builders<Refund>.Filter.Eq(r => r.Status.ToString(), status);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? statusFilter
            : Builders<Refund>.Filter.And(statusFilter, TenantFilter(tenantId));

        return await collection.Find(filter).ToListAsync(ct);
    }

    public async Task CreateAsync(Tenant tenant, Refund refund, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        await collection.InsertOneAsync(refund, cancellationToken: ct);
    }

    public async Task UpdateAsync(Tenant tenant, Refund refund, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Refund>.Filter.Eq(r => r.RefundId, refund.RefundId)
            : Builders<Refund>.Filter.And(
                Builders<Refund>.Filter.Eq(r => r.RefundId, refund.RefundId),
                TenantFilter(tenant.TenantId));

        await collection.ReplaceOneAsync(filter, refund, cancellationToken: ct);
    }
}
