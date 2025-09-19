using Microsoft.AspNetCore.Mvc;
using MusicSharing.Api.DTOs;
using MusicSharing.Api.Services;

namespace MusicSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaylistController(PlaylistService playlistService) : ControllerBase
{
    private readonly PlaylistService _playlistService = playlistService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var playlists = await _playlistService.GetAllAsync();
        return Ok(playlists);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var playlist = await _playlistService.GetByIdAsync(id);
        if (playlist == null) return NotFound();
        return Ok(playlist);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlaylistDto dto)
    {
        var playlist = await _playlistService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = playlist.Id }, playlist);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] int userId)
    {
        var deleted = await _playlistService.DeleteAsync(id, userId);
        if (!deleted) return Forbid();
        return NoContent();
    }

    // PUT: api/playlist/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreatePlaylistDto dto)
    {
        var updated = await _playlistService.UpdateAsync(id, dto.Name, dto.Description);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    // POST: api/playlist/{playlistId}/songs/{songId}
    [HttpPost("{playlistId}/songs/{songId}")]
    public async Task<IActionResult> AddSong(int playlistId, int songId)
    {
        var success = await _playlistService.AddSongAsync(playlistId, songId);
        if (!success) return NotFound();
        return NoContent();
    }

    // DELETE: api/playlist/{playlistId}/songs/{songId}
    [HttpDelete("{playlistId}/songs/{songId}")]
    public async Task<IActionResult> RemoveSong(int playlistId, int songId)
    {
        var success = await _playlistService.RemoveSongAsync(playlistId, songId);
        if (!success) return NotFound();
        return NoContent();
    }
}