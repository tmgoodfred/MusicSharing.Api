using MusicSharing.Api.Models;

namespace MusicSharing.Api.Services.Interfaces;

public interface IMusicService
{
    Task<List<Song>> GetAllSongsAsync();
    Task<Song?> GetSongByIdAsync(int id);
    Task<Song> CreateSongAsync(Song song);
    Task<Song?> UpdateSongAsync(int id, Song song);
    Task<bool> DeleteSongAsync(int id);

    Task<Song> UploadSongAsync(IFormFile file, Song songMetadata, int userId, IFormFile? artwork);

    Task<List<Song>> SearchAsync(string? title = null, string? artist = null, List<int>? categoryIds = null, int? userId = null);

    Task<Stream?> StreamSongFileAsync(string filePath);
}
