using System.ComponentModel.DataAnnotations;

namespace MusicSharing.Api.Models;

public class Comment
{
    public int Id { get; set; }

    [Required]
    public int SongId { get; set; }

    public Song? Song { get; set; }

    public int? UserId { get; set; } // Nullable for anonymous

    public User? User { get; set; }

    [Required]
    public string CommentText { get; set; } = default!;

    public bool IsAnonymous { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
