using MongoDB.Driver;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Repositories;
using MultiTenantShop.Infrastructure.Persistence;

namespace MultiTenantShop.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly TenantStore _tenantStore;

    public PaymentRepository(TenantStore tenantStore)
    {
        _tenantStore = tenantStore;
    }

    private IMongoCollection<Payment> GetCollection(Tenant tenant)
    {
        return _tenantStore.GetCollection<Payment>(tenant, "Payments");
    }

    private FilterDefinition<Payment> TenantFilter(string tenantId)
    {
        return Builders<Payment>.Filter.Eq(p => p.TenantId, tenantId);
    }

    public async Task<Payment?> GetByIdAsync(string tenantId, string paymentId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Payment>.Filter.Eq(p => p.PaymentId, paymentId)
            : Builders<Payment>.Filter.And(
                Builders<Payment>.Filter.Eq(p => p.PaymentId, paymentId),
                TenantFilter(tenantId));

        return await collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<Payment?> GetByOrderAsync(string tenantId, string orderId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Payment>.Filter.Eq(p => p.OrderId, orderId)
            : Builders<Payment>.Filter.And(
                Builders<Payment>.Filter.Eq(p => p.OrderId, orderId),
                TenantFilter(tenantId));

        return await collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Payment>> GetByStatusAsync(string tenantId, string status, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<Payment>();

        var collection = GetCollection(tenant);
        var statusFilter = Builders<Payment>.Filter.Eq(p => p.Status.ToString(), status);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? statusFilter
            : Builders<Payment>.Filter.And(statusFilter, TenantFilter(tenantId));

        return await collection.Find(filter).ToListAsync(ct);
    }

    public async Task CreateAsync(Tenant tenant, Payment payment, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        await collection.InsertOneAsync(payment, cancellationToken: ct);
    }

    public async Task UpdateAsync(Tenant tenant, Payment payment, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Payment>.Filter.Eq(p => p.PaymentId, payment.PaymentId)
            : Builders<Payment>.Filter.And(
                Builders<Payment>.Filter.Eq(p => p.PaymentId, payment.PaymentId),
                TenantFilter(tenant.TenantId));

        await collection.ReplaceOneAsync(filter, payment, cancellationToken: ct);
    }
}