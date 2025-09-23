namespace MusicSharing.Api.DTOs;

public class GlobalSearchResultDto
{
    public List<UserProfileDto> Users { get; set; } = [];
    public List<SongSearchResultDto> Songs { get; set; } = [];
}