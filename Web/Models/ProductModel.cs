using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class ProductModel(
    string brand,
    string name,
    DateTime? expiry,
    ExpiryType? expiryType,
    bool perishable,
    Guid locationId)
{
    public ProductModel(string brand, string name, DateTime? expiry, ExpiryType? expiryType, bool perishable, LocationModel location)
        : this(brand, name, expiry, expiryType, perishable, location.Id)
    {
    }

    public ProductModel(Guid id, string brand, string name, DateTime? expiry, ExpiryType? expiryType, bool perishable, LocationModel location)
        : this(brand, name, expiry, expiryType, perishable, location)
    {
        Id = id;
    }

    public ProductModel(Guid id, string brand, string name, DateTime? expiry, ExpiryType? expiryType, bool perishable, Guid locationId)
        : this(brand, name, expiry, expiryType, perishable, locationId)
    {
        Id = id;
    }

    [Required]
    public Guid Id { get; set; } = new();

    [Required]
    [StringLength(100)]
    public string Brand { get; set; } = brand;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = name;

    [Required]
    public Guid LocationId { get; set; } = locationId;

    public DateTime? Expiry { get; set; } = expiry;
    public ExpiryType? ExpiryType { get; set; } = expiryType;

    public bool Perishable { get; set; } = perishable;
}

public enum ExpiryType
{
    Expiration = 1,
    BestBy = 2
}