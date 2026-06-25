using MultiTenantShop.Core.Interfaces;
using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Core.Entities;

public class Customer : ITenantScoped
{
    private readonly List<Address> _addresses = [];

    public string CustomerId { get; private set; }
    public string TenantId { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? Phone { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<Address> Addresses => _addresses.AsReadOnly();

    public Customer(
        string customerId,
        string tenantId,
        string email,
        string firstName,
        string lastName,
        string? phone = null)
    {
        CustomerId = customerId;
        TenantId = tenantId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddAddress(Address address)
    {
        _addresses.Add(address);
    }

    public Address GetDefaultAddress()
    {
        return _addresses.Count > 0
            ? _addresses[0]
            : throw new InvalidOperationException("No addresses found");
    }
}
