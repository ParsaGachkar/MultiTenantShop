using MongoDB.Driver;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Repositories;
using MultiTenantShop.Infrastructure.Persistence;

namespace MultiTenantShop.Infrastructure.Repositories;

public class ShipmentRepository : IShipmentRepository
{
    private readonly TenantStore _tenantStore;

    public ShipmentRepository(TenantStore tenantStore)
    {
        _tenantStore = tenantStore;
    }

    private IMongoCollection<Shipment> GetCollection(Tenant tenant)
    {
        return _tenantStore.GetCollection<Shipment>(tenant, "Shipments");
    }

    private FilterDefinition<Shipment> TenantFilter(string tenantId)
    {
        return Builders<Shipment>.Filter.Eq(s => s.TenantId, tenantId);
    }

    public async Task<Shipment?> GetByIdAsync(string tenantId, string shipmentId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Shipment>.Filter.Eq(s => s.ShipmentId, shipmentId)
            : Builders<Shipment>.Filter.And(
                Builders<Shipment>.Filter.Eq(s => s.ShipmentId, shipmentId),
                TenantFilter(tenantId));

        return await collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Shipment>> GetByOrderAsync(string tenantId, string orderId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<Shipment>();

        var collection = GetCollection(tenant);
        var orderFilter = Builders<Shipment>.Filter.Eq(s => s.OrderId, orderId);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? orderFilter
            : Builders<Shipment>.Filter.And(orderFilter, TenantFilter(tenantId));

        return await collection.Find(filter).ToListAsync(ct);
    }

    public async Task CreateAsync(Tenant tenant, Shipment shipment, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        await collection.InsertOneAsync(shipment, cancellationToken: ct);
    }

    public async Task UpdateAsync(Tenant tenant, Shipment shipment, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Shipment>.Filter.Eq(s => s.ShipmentId, shipment.ShipmentId)
            : Builders<Shipment>.Filter.And(
                Builders<Shipment>.Filter.Eq(s => s.ShipmentId, shipment.ShipmentId),
                TenantFilter(tenant.TenantId));

        await collection.ReplaceOneAsync(filter, shipment, cancellationToken: ct);
    }
}
