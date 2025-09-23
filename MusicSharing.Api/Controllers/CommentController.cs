using Microsoft.AspNetCore.Mvc;
using MusicSharing.Api.DTOs;
using MusicSharing.Api.Models;
using MusicSharing.Api.Services;

namespace MusicSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController(CommentService commentService, ActivityService activityService) : ControllerBase
{
    private readonly CommentService _commentService = commentService;
    private readonly ActivityService _activityService = activityService;

    // GET: api/comment/song/{songId}
    [HttpGet("song/{songId}")]
    public async Task<IActionResult> GetBySong(int songId)
    {
        var comments = await _commentService.GetCommentsBySongAsync(songId);
        return Ok(comments);
    }

    // GET: api/comment/song/{songId}/{commentId}
    [HttpGet("song/{songId}/{commentId}")]
    public async Task<IActionResult> GetBySongComment(int songId, int commentId)
    {
        var comment = await _commentService.GetCommentBySongAndIdAsync(songId, commentId);
        if (comment is null) return NotFound();
        return Ok(comment);
    }

    // GET: api/comment/blog/{blogPostId}
    [HttpGet("blog/{blogPostId}")]
    public async Task<IActionResult> GetByBlogPost(int blogPostId)
    {
        var comments = await _commentService.GetCommentsByBlogPostAsync(blogPostId);
        return Ok(comments);
    }

    // GET: api/comment/blog/{blogPostId}/{commentId}
    [HttpGet("blog/{blogPostId}/{commentId}")]
    public async Task<IActionResult> GetByBlogPostComment(int blogPostId, int commentId)
    {
        var comment = await _commentService.GetCommentByBlogPostAndIdAsync(blogPostId, commentId);
        if (comment is null) return NotFound();
        return Ok(comment);
    }

    // POST: api/comment
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateCommentDto dto)
    {
        var comment = new Comment
        {
            SongId = dto.SongId,
            BlogPostId = dto.BlogPostId,
            CommentText = dto.CommentText,
            IsAnonymous = dto.IsAnonymous,
            UserId = dto.UserId,
            CreatedAt = DateTime.UtcNow
        };
        var created = await _commentService.AddCommentAsync(comment);
        return CreatedAtAction(
            comment.SongId.HasValue ? nameof(GetBySong) : nameof(GetByBlogPost),
            comment.SongId.HasValue ? new { songId = created.SongId } : new { blogPostId = created.BlogPostId },
            created
        );
    }

    // DELETE: api/comment/{commentId}
    [HttpDelete("{commentId}")]
    public async Task<IActionResult> Delete(int commentId, [FromQuery] int? userId = null, [FromQuery] bool isAdmin = false)
    {
        var deleted = await _commentService.DeleteCommentAsync(commentId, userId, isAdmin);
        if (!deleted) return Forbid();

        // Purge activity entries for this comment
        await _activityService.DeleteByCommentAsync(commentId);

        return NoContent();
    }
}