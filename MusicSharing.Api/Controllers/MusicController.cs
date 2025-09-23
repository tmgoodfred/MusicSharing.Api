using Microsoft.AspNetCore.Mvc;
using MusicSharing.Api.DTOs;
using MusicSharing.Api.Models;
using MusicSharing.Api.Services.Interfaces;

namespace MusicSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MusicController(IMusicService musicService) : ControllerBase
{
    private readonly IMusicService _musicService = musicService;

    // GET: api/music
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var songs = await _musicService.GetAllSongsAsync();
        return Ok(songs);
    }

    // GET api/music/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var song = await _musicService.GetSongByIdAsync(id);
        if (song == null) return NotFound();
        return Ok(song);
    }

    // POST: api/music
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Song song)
    {
        var created = await _musicService.CreateSongAsync(song);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
    int id,
    [FromForm] string title,
    [FromForm] string artist,
    [FromForm] string? genre,
    [FromForm] string? tags,
    [FromForm] IFormFile? artwork)
    {
        var song = await _musicService.GetSongByIdAsync(id);
        if (song == null) return NotFound();

        song.Title = title;
        song.Artist = artist;
        song.Genre = genre;
        song.Tags = tags?.Split(',').ToList();

        // Handle artwork upload
        if (artwork != null && artwork.Length > 0)
        {
            var updatedArtworkPath = await _musicService.SaveArtworkAsync(artwork);
            song.ArtworkPath = updatedArtworkPath;
        }

        var updated = await _musicService.UpdateSongAsync(id, song);
        return Ok(updated);
    }

    // DELETE: api/music/{id}?userId=123
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] int userId)
    {
        var song = await _musicService.GetSongByIdAsync(id);
        if (song == null) return NotFound();

        if (song.UserId != userId)
            return Forbid();

        var deleted = await _musicService.DeleteSongAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // GET: api/music/{id}/stream
    [HttpGet("{id}/stream")]
    public async Task<IActionResult> Stream(int id)
    {
        var song = await _musicService.GetSongByIdAsync(id);
        if (song == null) return NotFound();

        // Increment play count
        song.PlayCount++;
        await _musicService.UpdateSongAsync(song.Id, song);

        var stream = await _musicService.StreamSongFileAsync(song.FilePath);
        if (stream == null) return NotFound("Audio file not found.");

        return File(stream, "audio/mpeg", enableRangeProcessing: true);
    }

    // POST api/music/upload
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] SongUploadDto dto)
    {
        int userId = dto.UserId;

        var song = new Song
        {
            Title = dto.Title,
            Artist = dto.Artist,
            Genre = dto.Genre,
            Tags = dto.Tags
        };

        var created = await _musicService.UploadSongAsync(dto.File, song, userId, dto.Artwork);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // GET: api/music/{id}/artwork
    [HttpGet("{id}/artwork")]
    public IActionResult GetArtwork(int id)
    {
        var song = _musicService.GetSongByIdAsync(id).Result;
        if (song == null || string.IsNullOrEmpty(song.ArtworkPath))
            return NotFound("Artwork not found.");

        if (!System.IO.File.Exists(song.ArtworkPath))
            return NotFound("Artwork file missing on disk.");

        var mimeType = "image/jpeg"; // adjust if needed
        return PhysicalFile(song.ArtworkPath, mimeType);
    }

    // GET: api/music/search?title=...&artist=...&uploader=...&genre=...&...
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? title,
        [FromQuery] string? artist,
        [FromQuery] string? genre,
        [FromQuery] int? minPlays,
        [FromQuery] int? maxPlays,
        [FromQuery] double? minRating,
        [FromQuery] double? maxRating,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] List<string>? tags,
        [FromQuery] List<int>? categoryIds,
        [FromQuery] string? uploader
    )
    {
        var songs = await _musicService.AdvancedSearchAsync(
            title, artist, genre,
            minPlays, maxPlays,
            minRating, maxRating,
            fromDate, toDate,
            tags, categoryIds,
            uploader
        );

        var result = songs.Select(s => new SongSearchResultDto
        {
            Id = s.Id,
            Title = s.Title,
            Artist = s.Artist,
            Genre = s.Genre,
            Tags = s.Tags,
            UploadDate = s.UploadDate,
            PlayCount = s.PlayCount,
            DownloadCount = s.DownloadCount,
            Uploader = s.User == null ? null : new UserProfileDto
            {
                Id = s.User.Id,
                Username = s.User.Username,
                Email = s.User.Email,
                Role = s.User.Role.ToString(),
                CreatedAt = s.User.CreatedAt,
                ProfilePicturePath = s.User.ProfilePicturePath
            }
        }).ToList();

        return Ok(result);
    }

    // GET: api/music/{id}/download
    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var song = await _musicService.GetSongByIdAsync(id);
        if (song == null || string.IsNullOrEmpty(song.FilePath))
            return NotFound("Song or file not found.");

        if (!System.IO.File.Exists(song.FilePath))
            return NotFound("Audio file missing on disk.");

        // Increment download count
        song.DownloadCount++;
        await _musicService.UpdateSongAsync(song.Id, song);

        var fileName = System.IO.Path.GetFileName(song.FilePath);
        var mimeType = "audio/mpeg";

        var stream = new FileStream(song.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(stream, mimeType, fileName);
    }

    [HttpGet("user/{userId}/songs")]
    public async Task<IActionResult> GetUserSongs(int userId)
    {
        var songs = await _musicService.GetUserSongsAsync(userId);
        return Ok(songs);
    }

}
