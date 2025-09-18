using System.ComponentModel.DataAnnotations;

namespace MusicSharing.Api.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = default!;  // e.g., "Metal", "Chill"

    public string? Description { get; set; }

    // Many-to-many relationship
    public List<Song>? Songs { get; set; }
}
