namespace MusicSharing.Api.DTOs;

public class UserProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string? ProfilePicturePath { get; set; }
    public bool EmailConfirmed { get; set; }
}