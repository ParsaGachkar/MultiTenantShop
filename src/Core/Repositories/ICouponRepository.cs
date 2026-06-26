using MultiTenantShop.Core.Entities;

namespace MultiTenantShop.Core.Repositories;

public interface ICouponRepository
{
    Task<Coupon?> GetByIdAsync(string tenantId, string couponId, CancellationToken ct = default);
    Task<Coupon?> GetByCodeAsync(string tenantId, string code, CancellationToken ct = default);
    Task<IReadOnlyList<Coupon>> GetAllAsync(string tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Coupon>> GetActiveAsync(string tenantId, CancellationToken ct = default);
    Task CreateAsync(Tenant tenant, Coupon coupon, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, Coupon coupon, CancellationToken ct = default);
    Task DeleteAsync(Tenant tenant, string couponId, CancellationToken ct = default);
}