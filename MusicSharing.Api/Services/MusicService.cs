﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MusicSharing.Api.Data;
using MusicSharing.Api.DTOs;
using MusicSharing.Api.Models;
using MusicSharing.Api.Services.Interfaces;

namespace MusicSharing.Api.Services;

public class MusicService : IMusicService
{
    private readonly AppDbContext _context;
    private readonly string _audioFolder;
    private readonly string _artworkFolder;

    public MusicService(AppDbContext context, IConfiguration config)
    {
        _context = context;

        // Check if running on Linux (Unraid)
        if (OperatingSystem.IsLinux())
        {
            _audioFolder = config["FileStorage:LocalAudioPath"]!;
            _artworkFolder = config["FileStorage:LocalArtworkPath"]!;
        }
        else
        {
            // Assume Windows / remote access
            _audioFolder = config["FileStorage:NetworkAudioPath"]!;
            _artworkFolder = config["FileStorage:NetworkArtworkPath"]!;
        }
    }

    public async Task<List<Song>> GetAllSongsAsync()
    {
        return await _context.Songs
            .Include(s => s.Categories)
            .Include(s => s.Ratings)
            .Include(s => s.Comments)
            .ToListAsync();
    }

    public async Task<Song?> GetSongByIdAsync(int id)
    {
        return await _context.Songs
            .Include(s => s.Categories)
            .Include(s => s.Ratings)
            .Include(s => s.Comments)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Song> CreateSongAsync(Song song)
    {
        _context.Songs.Add(song);
        await _context.SaveChangesAsync();
        return song;
    }

    public async Task<Song?> UpdateSongAsync(int id, Song song)
    {
        var existingSong = await _context.Songs.FindAsync(id);
        if (existingSong == null) return null;

        existingSong.Title = song.Title;
        existingSong.Artist = song.Artist;
        existingSong.FilePath = song.FilePath;
        existingSong.ArtworkPath = song.ArtworkPath;
        existingSong.Genre = song.Genre;
        existingSong.Tags = song.Tags;
        existingSong.Categories = song.Categories;

        await _context.SaveChangesAsync();
        return existingSong;
    }

    public async Task<bool> DeleteSongAsync(int id)
    {
        var song = await _context.Songs.FindAsync(id);
        if (song == null) return false;

        _context.Songs.Remove(song);
        await _context.SaveChangesAsync();
        return true;
    }

    // Stream audio file from Unraid share
    public async Task<Stream?> StreamSongFileAsync(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await Task.FromResult(stream);
    }

    public async Task<Song> UploadSongAsync(IFormFile file, Song songMetadata, int userId, IFormFile? artwork = null)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No audio file provided.");

        // --- Save Audio File ---
        if (!Directory.Exists(_audioFolder))
            Directory.CreateDirectory(_audioFolder);

        var audioFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var audioFilePath = Path.Combine(_audioFolder, audioFileName);

        using (var stream = new FileStream(audioFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        songMetadata.FilePath = audioFilePath;
        songMetadata.UserId = userId;

        // --- Save Artwork File ---
        if (artwork != null && artwork.Length > 0)
        {
            if (!Directory.Exists(_artworkFolder))
                Directory.CreateDirectory(_artworkFolder);

            var artworkFileName = $"{Guid.NewGuid()}_{artwork.FileName}";
            var artworkFilePath = Path.Combine(_artworkFolder, artworkFileName);

            using (var stream = new FileStream(artworkFilePath, FileMode.Create))
            {
                await artwork.CopyToAsync(stream);
            }

            songMetadata.ArtworkPath = artworkFilePath;
        }

        // --- Save to Database ---
        _context.Songs.Add(songMetadata);
        await _context.SaveChangesAsync();

        return songMetadata;
    }

    public async Task<List<Song>> SearchAsync(string? title = null, string? artist = null, List<int>? categoryIds = null, int? userId = null)
    {
        var query = _context.Songs
            .Include(s => s.Categories)
            .Include(s => s.Ratings)
            .Include(s => s.Comments)
            .AsQueryable();

        if (!string.IsNullOrEmpty(title))
            query = query.Where(s => s.Title.Contains(title));

        if (!string.IsNullOrEmpty(artist))
            query = query.Where(s => s.Artist.Contains(artist));

        if (categoryIds != null && categoryIds.Count > 0)
            query = query.Where(s => (s.Categories != null && s.Categories.Count > 0 && s.Categories.Any(c => categoryIds.Contains(c.Id))));

        if (userId.HasValue)
            query = query.Where(s => s.UserId == userId.Value);

        return await query.ToListAsync();
    }

    public async Task<(Stream? Stream, string? FileName)> DownloadSongFileAsync(int songId)
    {
        var song = await GetSongByIdAsync(songId);
        if (song == null || string.IsNullOrEmpty(song.FilePath) || !File.Exists(song.FilePath))
            return (null, null);

        var stream = new FileStream(song.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var fileName = Path.GetFileName(song.FilePath);
        return (stream, fileName);
    }

    public async Task<UserSongAnalyticsDto> GetUserSongAnalyticsAsync(int userId)
    {
        var songs = await _context.Songs
            .Where(s => s.UserId == userId)
            .Include(s => s.Ratings)
            .ToListAsync();

        var totalSongs = songs.Count;
        var totalPlays = songs.Sum(s => s.PlayCount);
        var allRatings = songs.SelectMany(s => s.Ratings ?? []);
        var averageRating = allRatings.Any() ? allRatings.Average(r => r.RatingValue) : 0.0;
        var totalDownloads = songs.Sum(s => s.DownloadCount); // If you track downloads

        var mostPopular = songs.OrderByDescending(s => s.PlayCount).FirstOrDefault();
        var mostRecent = songs.OrderByDescending(s => s.UploadDate).FirstOrDefault();

        return new UserSongAnalyticsDto
        {
            UserId = userId,
            TotalSongs = totalSongs,
            TotalPlays = totalPlays,
            AverageRating = averageRating,
            TotalDownloads = totalDownloads,
            MostPopularSongTitle = mostPopular?.Title,
            MostPopularSongId = mostPopular?.Id,
            MostPopularSongPlays = mostPopular?.PlayCount,
            MostRecentSongTitle = mostRecent?.Title,
            MostRecentSongId = mostRecent?.Id,
            MostRecentUploadDate = mostRecent?.UploadDate
        };
    }

    public async Task<List<Song>> AdvancedSearchAsync(
    string? title, string? artist, string? genre,
    int? minPlays, int? maxPlays,
    double? minRating, double? maxRating,
    DateTime? fromDate, DateTime? toDate,
    List<string>? tags, List<int>? categoryIds)
    {
        var query = _context.Songs
            .Include(s => s.Categories)
            .Include(s => s.Ratings)
            .Include(s => s.Comments)
            .AsQueryable();

        if (!string.IsNullOrEmpty(title))
            query = query.Where(s => s.Title.Contains(title));
        if (!string.IsNullOrEmpty(artist))
            query = query.Where(s => s.Artist.Contains(artist));
        if (!string.IsNullOrEmpty(genre))
            query = query.Where(s => s.Genre == genre);
        if (minPlays.HasValue)
            query = query.Where(s => s.PlayCount >= minPlays.Value);
        if (maxPlays.HasValue)
            query = query.Where(s => s.PlayCount <= maxPlays.Value);
        if (fromDate.HasValue)
            query = query.Where(s => s.UploadDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(s => s.UploadDate <= toDate.Value);
        if (tags != null && tags.Count > 0)
            query = query.Where(s => s.Tags != null && s.Tags.Any(t => tags.Contains(t)));
        if (categoryIds != null && categoryIds.Count > 0)
            query = query.Where(s => s.Categories != null && s.Categories.Any(c => categoryIds.Contains(c.Id)));
        if (minRating.HasValue)
            query = query.Where(s => s.Ratings != null && s.Ratings.Any() && s.Ratings.Average(r => r.RatingValue) >= minRating.Value);
        if (maxRating.HasValue)
            query = query.Where(s => s.Ratings != null && s.Ratings.Any() && s.Ratings.Average(r => r.RatingValue) <= maxRating.Value);

        return await query.ToListAsync();
    }

}
