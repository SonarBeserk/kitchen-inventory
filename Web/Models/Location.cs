using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class Location(string name, string description)
{
    [Required]
    public Guid Id { get; set; } = new();

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = name;

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = description;
}