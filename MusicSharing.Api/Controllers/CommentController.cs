using Microsoft.AspNetCore.Mvc;
using MusicSharing.Api.DTOs;
using MusicSharing.Api.Models;
using MusicSharing.Api.Services;

namespace MusicSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly CommentService _commentService;

    public CommentController(CommentService commentService)
    {
        _commentService = commentService;
    }

    // GET: api/comment/song/{songId}
    [HttpGet("song/{songId}")]
    public async Task<IActionResult> GetBySong(int songId)
    {
        var comments = await _commentService.GetCommentsBySongAsync(songId);
        return Ok(comments);
    }

    // POST: api/comment
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateCommentDto dto)
    {
        var comment = new Comment
        {
            SongId = dto.SongId,
            CommentText = dto.CommentText,
            IsAnonymous = dto.IsAnonymous,
            UserId = dto.UserId,
            CreatedAt = DateTime.UtcNow
        };
        var created = await _commentService.AddCommentAsync(comment);
        return CreatedAtAction(nameof(GetBySong), new { songId = created.SongId }, created);
    }

    // DELETE: api/comment/{commentId}
    [HttpDelete("{commentId}")]
    public async Task<IActionResult> Delete(int commentId, [FromQuery] int? userId = null, [FromQuery] bool isAdmin = false)
    {
        var deleted = await _commentService.DeleteCommentAsync(commentId, userId, isAdmin);
        if (!deleted) return Forbid();
        return NoContent();
    }
}