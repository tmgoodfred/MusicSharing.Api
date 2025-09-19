using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSharing.Api.Models;

public class Playlist
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public ICollection<Song>? Songs { get; set; }
}