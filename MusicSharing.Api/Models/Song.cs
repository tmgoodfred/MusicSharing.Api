using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSharing.Api.Models;

public class Song
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = default!;

    [MaxLength(100)]
    public string Artist { get; set; } = "Unknown";

    [Required]
    public string FilePath { get; set; } = default!; // Path on Unraid

    public string? ArtworkPath { get; set; } // Optional cover image

    [MaxLength(50)]
    public string? Genre { get; set; }

    [Column(TypeName = "text[]")]
    public List<string>? Tags { get; set; } // Postgres array

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public int PlayCount { get; set; } = 0;

    // Many-to-many relationship
    public List<Category>? Categories { get; set; }

    // Navigation properties
    public List<Comment>? Comments { get; set; }
    public List<Rating>? Ratings { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }
}
