using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class Location(string name, string description)
{
    public Location(Guid id, string name, string description)
        : this(name, description)
    {
        Id = id;
    }

    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = name;

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = description;
}
