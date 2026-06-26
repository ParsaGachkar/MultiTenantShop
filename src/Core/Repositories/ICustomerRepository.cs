using MultiTenantShop.Core.Entities;

namespace MultiTenantShop.Core.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(string tenantId, string customerId, CancellationToken ct = default);
    Task<Customer?> GetByEmailAsync(string tenantId, string email, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetAllAsync(string tenantId, CancellationToken ct = default);
    Task CreateAsync(Tenant tenant, Customer customer, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, Customer customer, CancellationToken ct = default);
}