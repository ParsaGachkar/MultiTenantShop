namespace MultiTenantShop.Core.Entities;

public class Tenant
{
    public string TenantId { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string Plan { get; private set; }
    public string DefaultCurrency { get; private set; }
    public string DefaultLanguage { get; private set; }
    public List<string> SupportedCurrencies { get; private set; }
    public string? CustomDomain { get; private set; }
    public string? ThemeSettings { get; private set; }
    public string? ConnectionString { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Tenant(
        string tenantId,
        string name,
        string slug,
        string plan,
        string defaultCurrency,
        string defaultLanguage,
        List<string>? supportedCurrencies = null)
    {
        TenantId = tenantId;
        Name = name;
        Slug = slug;
        Plan = plan;
        DefaultCurrency = defaultCurrency;
        DefaultLanguage = defaultLanguage;
        SupportedCurrencies = supportedCurrencies ?? [defaultCurrency];
        CreatedAt = DateTime.UtcNow;
    }
}
