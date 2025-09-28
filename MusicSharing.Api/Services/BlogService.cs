using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Data;
using MusicSharing.Api.Models;

namespace MusicSharing.Api.Services;

public class BlogService(AppDbContext context, ActivityService activityService)
{
    private readonly AppDbContext _context = context;
    private readonly ActivityService _activityService = activityService;

    public async Task<List<BlogPost>> GetAllAsync()
    {
        return await _context.BlogPosts
            .Include(p => p.Author)
            .OrderByDescending(p => p.PublishDate)
            .ToListAsync();
    }

    public async Task<BlogPost?> GetByIdAsync(int id)
    {
        return await _context.BlogPosts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<BlogPost> CreateAsync(BlogPost post)
    {
        post.PublishDate = DateTime.UtcNow;
        _context.BlogPosts.Add(post);
        await _context.SaveChangesAsync();

        // Log activity for blog post creation
        if (post.AuthorId.HasValue)
        {
            await _activityService.AddAsync(new Activity
            {
                UserId = post.AuthorId.Value,
                Type = "BlogPost",
                Data = $"{{\"BlogPostId\":{post.Id},\"Title\":\"{post.Title}\"}}",
                CreatedAt = DateTime.UtcNow
            });
        }

        return post;
    }

    public async Task<BlogPost?> UpdateAsync(int id, BlogPost updated)
    {
        var post = await _context.BlogPosts.FindAsync(id);
        if (post == null) return null;

        post.Title = updated.Title;
        post.Content = updated.Content;
        post.AuthorId = updated.AuthorId;
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var post = await _context.BlogPosts.FindAsync(id);
        if (post == null) return false;

        // Delete related comments and their activities
        var comments = await _context.Comments.Where(c => c.BlogPostId == id).ToListAsync();
        foreach (var comment in comments)
        {
            await _activityService.DeleteByCommentAsync(comment.Id);
            _context.Comments.Remove(comment);
        }

        // Delete related activity entries (e.g., post created or commented on)
        await _activityService.DeleteByBlogPostAsync(id);

        _context.BlogPosts.Remove(post);
        await _context.SaveChangesAsync();
        return true;
    }
}