using MongoDB.Driver;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Repositories;
using MultiTenantShop.Infrastructure.Persistence;

namespace MultiTenantShop.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly TenantStore _tenantStore;

    public OrderRepository(TenantStore tenantStore)
    {
        _tenantStore = tenantStore;
    }

    private IMongoCollection<Order> GetCollection(Tenant tenant)
    {
        return _tenantStore.GetCollection<Order>(tenant, "Orders");
    }

    private FilterDefinition<Order> TenantFilter(string tenantId)
    {
        return Builders<Order>.Filter.Eq(o => o.TenantId, tenantId);
    }

    public async Task<Order?> GetByIdAsync(string tenantId, string orderId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Order>.Filter.Eq(o => o.OrderId, orderId)
            : Builders<Order>.Filter.And(
                Builders<Order>.Filter.Eq(o => o.OrderId, orderId),
                TenantFilter(tenantId));

        return await collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerAsync(string tenantId, string customerId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<Order>();

        var collection = GetCollection(tenant);
        var customerFilter = Builders<Order>.Filter.Eq(o => o.CustomerId, customerId);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? customerFilter
            : Builders<Order>.Filter.And(customerFilter, TenantFilter(tenantId));

        return await collection.Find(filter)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(string tenantId, string status, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<Order>();

        var collection = GetCollection(tenant);
        var statusFilter = Builders<Order>.Filter.Eq(o => o.Status.ToString(), status);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? statusFilter
            : Builders<Order>.Filter.And(statusFilter, TenantFilter(tenantId));

        return await collection.Find(filter)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(Tenant tenant, Order order, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        await collection.InsertOneAsync(order, cancellationToken: ct);
    }

    public async Task UpdateAsync(Tenant tenant, Order order, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Order>.Filter.Eq(o => o.OrderId, order.OrderId)
            : Builders<Order>.Filter.And(
                Builders<Order>.Filter.Eq(o => o.OrderId, order.OrderId),
                TenantFilter(tenant.TenantId));

        await collection.ReplaceOneAsync(filter, order, cancellationToken: ct);
    }
}