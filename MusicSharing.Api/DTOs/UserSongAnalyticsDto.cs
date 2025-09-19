namespace MusicSharing.Api.DTOs;

public class UserSongAnalyticsDto
{
    public int UserId { get; set; }
    public int TotalSongs { get; set; }
    public int TotalPlays { get; set; }
    public double AverageRating { get; set; }
    public int TotalDownloads { get; set; }
    public string? MostPopularSongTitle { get; set; }
    public int? MostPopularSongId { get; set; }
    public int? MostPopularSongPlays { get; set; }
    public string? MostRecentSongTitle { get; set; }
    public int? MostRecentSongId { get; set; }
    public DateTime? MostRecentUploadDate { get; set; }
}