using MongoDB.Driver;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Repositories;
using MultiTenantShop.Infrastructure.Persistence;

namespace MultiTenantShop.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly TenantStore _tenantStore;

    public CustomerRepository(TenantStore tenantStore)
    {
        _tenantStore = tenantStore;
    }

    private IMongoCollection<Customer> GetCollection(Tenant tenant)
    {
        return _tenantStore.GetCollection<Customer>(tenant, "Customers");
    }

    private FilterDefinition<Customer> TenantFilter(string tenantId)
    {
        return Builders<Customer>.Filter.Eq(c => c.TenantId, tenantId);
    }

    public async Task<Customer?> GetByIdAsync(string tenantId, string customerId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Customer>.Filter.Eq(c => c.CustomerId, customerId)
            : Builders<Customer>.Filter.And(
                Builders<Customer>.Filter.Eq(c => c.CustomerId, customerId),
                TenantFilter(tenantId));

        return await collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<Customer?> GetByEmailAsync(string tenantId, string email, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Customer>.Filter.Eq(c => c.Email, email)
            : Builders<Customer>.Filter.And(
                Builders<Customer>.Filter.Eq(c => c.Email, email),
                TenantFilter(tenantId));

        return await collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(string tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<Customer>();

        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? FilterDefinition<Customer>.Empty
            : TenantFilter(tenantId);

        return await collection.Find(filter).ToListAsync(ct);
    }

    public async Task CreateAsync(Tenant tenant, Customer customer, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        await collection.InsertOneAsync(customer, cancellationToken: ct);
    }

    public async Task UpdateAsync(Tenant tenant, Customer customer, CancellationToken ct = default)
    {
        var collection = GetCollection(tenant);
        var filter = !string.IsNullOrEmpty(tenant.ConnectionString)
            ? Builders<Customer>.Filter.Eq(c => c.CustomerId, customer.CustomerId)
            : Builders<Customer>.Filter.And(
                Builders<Customer>.Filter.Eq(c => c.CustomerId, customer.CustomerId),
                TenantFilter(tenant.TenantId));

        await collection.ReplaceOneAsync(filter, customer, cancellationToken: ct);
    }
}