using MultiTenantShop.Core.Interfaces;

namespace MultiTenantShop.Core.Entities;

public class Category : ITenantScoped
{
    public string CategoryId { get; private set; }
    public string TenantId { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }

    public Category(string categoryId, string tenantId, string name, string slug, string? description = null)
    {
        CategoryId = categoryId;
        TenantId = tenantId;
        Name = name;
        Slug = slug;
        Description = description;
    }
}
