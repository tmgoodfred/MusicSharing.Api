namespace MusicSharing.Api.DTOs;

public class UpdateProfileDto
{
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
}