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

    // PUT: api/music/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Song song)
    {
        var updated = await _musicService.UpdateSongAsync(id, song);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    // DELETE: api/music/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
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

        var stream = await _musicService.StreamSongFileAsync(song.FilePath);
        if (stream == null) return NotFound("Audio file not found.");

        // Return stream with correct MIME type
        return File(stream, "audio/mpeg", enableRangeProcessing: true);
    }

    // POST api/music/upload
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] SongUploadDto dto)
    {
        // Get userId from claims (example)
        int userId = int.Parse(User.FindFirst("sub")!.Value);

        var song = new Song
        {
            Title = dto.Title,
            Artist = dto.Artist
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

    // GET: api/music/search?title=...&artist=...&categoryIds=1,2,3
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? title, [FromQuery] string? artist, [FromQuery] List<int>? categoryIds)
    {
        var results = await _musicService.SearchAsync(title, artist, categoryIds);
        return Ok(results);
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

        var fileName = System.IO.Path.GetFileName(song.FilePath);
        var mimeType = "audio/mpeg"; // Adjust if you support other formats

        var stream = new FileStream(song.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(stream, mimeType, fileName);
    }

}
