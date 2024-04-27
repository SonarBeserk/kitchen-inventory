using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class Product(
    string brand,
    string name,
    DateTime? expiry,
    ExpiryType? expiryType,
    bool perishable)
{
    public Product(Guid id, string brand, string name, DateTime? expiry, ExpiryType? expiryType, bool perishable)
        : this(brand, name, expiry, expiryType, perishable)
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

    public DateTime? Expiry { get; set; } = expiry;
    public ExpiryType? ExpiryType { get; set; } = expiryType;

    public bool Perishable { get; set; } = perishable;
}

public enum ExpiryType
{
    Expiration = 1,
    BestBy = 2
}