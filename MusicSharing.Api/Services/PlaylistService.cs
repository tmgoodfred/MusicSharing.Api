using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Data;
using MusicSharing.Api.DTOs;
using MusicSharing.Api.Models;

namespace MusicSharing.Api.Services;

public class PlaylistService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<Playlist>> GetAllAsync()
    {
        return await _context.Playlists
            .Include(p => p.Songs)
            .Include(p => p.User)
            .ToListAsync();
    }

    public async Task<Playlist?> GetByIdAsync(int id)
    {
        return await _context.Playlists
            .Include(p => p.Songs)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Playlist> CreateAsync(CreatePlaylistDto dto)
    {
        var playlist = new Playlist
        {
            Name = dto.Name,
            Description = dto.Description,
            UserId = dto.UserId,
            Songs = dto.SongIds != null
                ? await _context.Songs.Where(s => dto.SongIds.Contains(s.Id)).ToListAsync()
                : [] //used to be : new List<Song>() if this causes issues.
        };
        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync();
        return playlist;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (playlist == null) return false;
        _context.Playlists.Remove(playlist);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Playlist?> UpdateAsync(int id, string? name, string? description)
    {
        var playlist = await _context.Playlists.FindAsync(id);
        if (playlist == null) return null;

        if (!string.IsNullOrWhiteSpace(name))
            playlist.Name = name;
        if (description != null)
            playlist.Description = description;

        await _context.SaveChangesAsync();
        return playlist;
    }

    public async Task<bool> AddSongAsync(int playlistId, int songId)
    {
        var playlist = await _context.Playlists
            .Include(p => p.Songs)
            .FirstOrDefaultAsync(p => p.Id == playlistId);
        var song = await _context.Songs.FindAsync(songId);

        if (playlist == null || song == null) return false;
        playlist.Songs ??= []; //used to be if (playlist.Songs == null) playlist.Songs = []; if this breaks

        if (!playlist.Songs.Any(s => s.Id == songId))
            playlist.Songs.Add(song);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveSongAsync(int playlistId, int songId)
    {
        var playlist = await _context.Playlists
            .Include(p => p.Songs)
            .FirstOrDefaultAsync(p => p.Id == playlistId);
        if (playlist == null || playlist.Songs == null) return false;

        var song = playlist.Songs.FirstOrDefault(s => s.Id == songId);
        if (song == null) return false;

        playlist.Songs.Remove(song);
        await _context.SaveChangesAsync();
        return true;
    }
}