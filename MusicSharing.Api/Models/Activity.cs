namespace MusicSharing.Api.Models;

public class Activity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = default!; // e.g., "Upload", "Comment", "Rating"
    public string? Data { get; set; } // JSON or description
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}