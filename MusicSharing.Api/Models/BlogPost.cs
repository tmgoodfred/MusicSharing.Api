using System.ComponentModel.DataAnnotations;

namespace MusicSharing.Api.Models;

public class BlogPost
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = default!;

    [Required]
    public string Content { get; set; } = default!;

    public int? AuthorId { get; set; }

    public User? Author { get; set; }

    public DateTime PublishDate { get; set; } = DateTime.UtcNow;
}
