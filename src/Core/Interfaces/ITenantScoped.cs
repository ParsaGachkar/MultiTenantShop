namespace MultiTenantShop.Core.Interfaces;

public interface ITenantScoped
{
    string TenantId { get; }
}
