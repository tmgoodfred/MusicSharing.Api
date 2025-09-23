using MusicSharing.Api.Models;

namespace MusicSharing.Api.DTOs;

public class SongSearchResultDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Artist { get; set; } = default!;
    public string? Genre { get; set; }
    public List<string>? Tags { get; set; }
    public DateTime UploadDate { get; set; }
    public int PlayCount { get; set; }
    public int DownloadCount { get; set; }

    public UserProfileDto? Uploader { get; set; }
}