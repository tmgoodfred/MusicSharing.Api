using System.ComponentModel.DataAnnotations;

namespace MusicSharing.Api.Models;

public class Rating
{
    public int Id { get; set; }

    [Required]
    public int SongId { get; set; }

    public Song? Song { get; set; }

    [Required]
    public int UserId { get; set; }

    public User? User { get; set; }

    [Range(1, 5)]
    public int RatingValue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
