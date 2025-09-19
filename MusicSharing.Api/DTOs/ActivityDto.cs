namespace MusicSharing.Api.DTOs;

public class ActivityDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = default!;
    public string? Data { get; set; }
    public DateTime CreatedAt { get; set; }
}