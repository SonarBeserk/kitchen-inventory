using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class Product(
    string brand,
    string name)
{
    public Product(Guid id, string brand, string name) :
        this(brand, name)
    {
        Id = id;
    }

    public Product(string brand, string name, Guid? locationId, bool perishable, long amount) :
        this(brand, name)
    {
        LocationId = locationId;
        Perishable = perishable;
        Amount = amount;
    }

    public Product(Guid id, string brand, string name, Guid? locationId, bool perishable, long amount) :
        this(id, brand, name)
    {
        LocationId = locationId;
        Perishable = perishable;
        Amount = amount;
    }

    public Product(string brand, string name, Guid? locationId, bool perishable, long amount, DateTime? expiry, ExpiryType? expiryType)
        : this(brand, name, locationId, perishable, amount)
    {
        Expiry = expiry;
        ExpiryType = expiryType;
    }

    public Product(Guid id, string brand, string name, Guid? locationId, bool perishable, long amount, DateTime? expiry, ExpiryType? expiryType)
        : this(brand, name, locationId, perishable, amount, expiry, expiryType)
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

    public bool Perishable { get; set; }

    public long Amount { get; set; }

    public Guid? LocationId { get; set; }
}

public enum ExpiryType
{
    Expiration = 1,
    BestBy = 2
}