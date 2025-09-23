using Microsoft.AspNetCore.Http;

namespace MusicSharing.Api.DTOs;

public class SongUpdateFormDto
{
    public string Title { get; set; } = default!;
    public string Artist { get; set; } = default!;
    public string? Genre { get; set; }
    public string? Tags { get; set; }
    public IFormFile? Artwork { get; set; }
}