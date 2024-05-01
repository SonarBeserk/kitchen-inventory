using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class Product(
    string brand,
    string name,
    bool perishable,
    long amount)
{
    public Product(string brand, string name, DateTime? expiry, ExpiryType? expiryType, bool perishable, long amount, Guid? locationId)
        : this(brand, name, perishable, amount)
    {
        Expiry = expiry;
        ExpiryType = expiryType;
        LocationId = locationId;
    }

    public Product(string brand, string name, DateTime? expiry, ExpiryType? expiryType, bool perishable, long amount)
        : this(brand, name, perishable, amount)
    {
        Expiry = expiry;
        ExpiryType = expiryType;
    }

    public Product(Guid id, string brand, string name, DateTime? expiry, ExpiryType? expiryType, bool perishable, long amount, Guid? locationId)
        : this(brand, name, expiry, expiryType, perishable, amount, locationId)
    {
        Id = id;
    }

    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Brand { get; set; } = brand;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = name;

    public DateTime? Expiry { get; set; }
    public ExpiryType? ExpiryType { get; set; }

    public bool Perishable { get; set; } = perishable;

    public long Amount { get; set; } = amount;

    public Guid? LocationId { get; set; }
}

public enum ExpiryType
{
    Expiration = 1,
    BestBy = 2
}