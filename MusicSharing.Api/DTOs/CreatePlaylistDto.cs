namespace MusicSharing.Api.DTOs;

public class CreatePlaylistDto
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int UserId { get; set; }
    public List<int>? SongIds { get; set; }
}