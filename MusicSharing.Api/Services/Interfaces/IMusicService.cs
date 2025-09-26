using MusicSharing.Api.Models;

namespace MusicSharing.Api.Services.Interfaces;

public interface IMusicService
{
    Task<List<Song>> GetAllSongsAsync();
    Task<Song?> GetSongByIdAsync(int id);
    Task<Song> CreateSongAsync(Song song);
    Task<Song?> UpdateSongAsync(int id, Song song);
    Task<bool> DeleteSongAsync(int id, int currentUserId);
    Task<List<Song>> GetUserSongsAsync(int userId);

    Task<Song> UploadSongAsync(IFormFile file, Song songMetadata, int userId, IFormFile? artwork);

    Task<string> SaveArtworkAsync(IFormFile artwork);

    Task<List<Song>> SearchAsync(string? title = null, string? artist = null, List<int>? categoryIds = null, int? userId = null);

    Task<(Stream? Stream, string? FileName)> DownloadSongFileAsync(int songId);

    Task<Stream?> StreamSongFileAsync(string filePath);
    Task<List<Song>> AdvancedSearchAsync(
        string? title, string? artist, string? genre,
        int? minPlays, int? maxPlays,
        double? minRating, double? maxRating,
        DateTime? fromDate, DateTime? toDate,
        List<string>? tags, List<int>? categoryIds,
        string? uploader = null
    );

}
