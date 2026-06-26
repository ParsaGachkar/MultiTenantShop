using MongoDB.Driver;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Repositories;
using MultiTenantShop.Infrastructure.Persistence;

namespace MultiTenantShop.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly MongoDbContext _context;

    public TenantRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _context.Tenants
            .Find(t => t.Slug == slug)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Tenant?> GetByIdAsync(string tenantId, CancellationToken ct = default)
    {
        return await _context.Tenants
            .Find(t => t.TenantId == tenantId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task CreateAsync(Tenant tenant, CancellationToken ct = default)
    {
        await _context.Tenants.InsertOneAsync(tenant, cancellationToken: ct);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        await _context.Tenants.ReplaceOneAsync(
            t => t.TenantId == tenant.TenantId,
            tenant,
            cancellationToken: ct);
    }

    public async Task DeleteAsync(string tenantId, CancellationToken ct = default)
    {
        await _context.Tenants.DeleteOneAsync(t => t.TenantId == tenantId, ct);
    }
}