using MultiTenantShop.Core.Entities;

namespace MultiTenantShop.Core.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(string tenantId, string paymentId, CancellationToken ct = default);
    Task<Payment?> GetByOrderAsync(string tenantId, string orderId, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetByStatusAsync(string tenantId, string status, CancellationToken ct = default);
    Task CreateAsync(Tenant tenant, Payment payment, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, Payment payment, CancellationToken ct = default);
}