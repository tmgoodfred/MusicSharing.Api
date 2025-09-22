using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Data;
using MusicSharing.Api.Models;

namespace MusicSharing.Api.Services;

public class CommentService(AppDbContext context, ActivityService activityService)
{
    private readonly AppDbContext _context = context;
    private readonly ActivityService _activityService = activityService;

    public async Task<List<Comment>> GetCommentsBySongAsync(int songId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Where(c => c.SongId == songId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Comment>> GetCommentsByBlogPostAsync(int blogPostId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Where(c => c.BlogPostId == blogPostId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Log activity
        if (comment.UserId.HasValue)
        {
            string data = comment.SongId.HasValue
                ? $"{{\"SongId\":{comment.SongId},\"CommentId\":{comment.Id}}}"
                : $"{{\"BlogPostId\":{comment.BlogPostId},\"CommentId\":{comment.Id}}}";

            await _activityService.AddAsync(new Activity
            {
                UserId = comment.UserId.Value,
                Type = "Comment",
                Data = data,
                CreatedAt = DateTime.UtcNow
            });
        }

        return comment;
    }

    public async Task<bool> DeleteCommentAsync(int commentId, int? userId = null, bool isAdmin = false)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null) return false;

        // Only allow delete if admin or comment owner
        if (!isAdmin && comment.UserId != userId) return false;

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }
}