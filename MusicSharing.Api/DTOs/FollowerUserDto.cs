namespace MusicSharing.Api.DTOs;

public class FollowerUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
}