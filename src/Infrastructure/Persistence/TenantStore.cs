using MongoDB.Driver;
using MultiTenantShop.Core.Entities;

namespace MultiTenantShop.Infrastructure.Persistence;

public class TenantStore
{
    private readonly MongoDbContext _context;

    public TenantStore(MongoDbContext context)
    {
        _context = context;
    }

    public virtual IMongoCollection<T> GetCollection<T>(Tenant tenant, string collectionName)
        where T : class
    {
        if (!string.IsNullOrEmpty(tenant.ConnectionString))
        {
            var database = _context.GetTenantDatabase(tenant.ConnectionString);
            return database.GetCollection<T>(collectionName);
        }

        var sharedDatabase = _context.GetSharedDatabase();
        return sharedDatabase.GetCollection<T>(collectionName);
    }

    public virtual async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _context.Tenants
            .Find(t => t.Slug == slug)
            .FirstOrDefaultAsync(ct);
    }

    public virtual async Task<Tenant?> GetByIdAsync(string tenantId, CancellationToken ct = default)
    {
        return await _context.Tenants
            .Find(t => t.TenantId == tenantId)
            .FirstOrDefaultAsync(ct);
    }
}