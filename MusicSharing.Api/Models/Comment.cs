using System.ComponentModel.DataAnnotations;

namespace MusicSharing.Api.Models;

public class Comment
{
    public int Id { get; set; }

    // Existing for song comments
    public int? SongId { get; set; }
    public Song? Song { get; set; }

    // New for blog post comments
    public int? BlogPostId { get; set; }
    public BlogPost? BlogPost { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    [Required]
    public string CommentText { get; set; } = string.Empty;

    public bool IsAnonymous { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
