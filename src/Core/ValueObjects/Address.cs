namespace MultiTenantShop.Core.ValueObjects;

public class Address : IEquatable<Address>
{
    public string Province { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Street { get; }
    public string FullText { get; }
    public decimal? Latitude { get; }
    public decimal? Longitude { get; }

    public Address(
        string province,
        string city,
        string postalCode,
        string street,
        string? fullText = null,
        decimal? latitude = null,
        decimal? longitude = null)
    {
        if (string.IsNullOrWhiteSpace(province))
            throw new ArgumentException("Province is required", nameof(province));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code is required", nameof(postalCode));
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required", nameof(street));

        Province = province;
        City = city;
        PostalCode = postalCode;
        Street = street;
        FullText = fullText ?? $"{street}, {city}, {province}";
        Latitude = latitude;
        Longitude = longitude;
    }

    public string Format(bool persian)
    {
        if (persian)
            return $"{Street}, {City}, {Province} — {PostalCode}";
        return $"{Street}, {City}, {Province} {PostalCode}";
    }

    public bool Equals(Address? other)
    {
        if (other is null) return false;
        return string.Equals(Province, other.Province, StringComparison.Ordinal) &&
               string.Equals(City, other.City, StringComparison.Ordinal) &&
               string.Equals(PostalCode, other.PostalCode, StringComparison.Ordinal) &&
               string.Equals(Street, other.Street, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) =>
        obj is Address other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(Province, City, PostalCode, Street);

    public static bool operator ==(Address? left, Address? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Address? left, Address? right) =>
        !(left == right);
}
