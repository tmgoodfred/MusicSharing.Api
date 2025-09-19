using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Data;
using MusicSharing.Api.Models;

namespace MusicSharing.Api.Services;

public class BlogService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

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

        _context.BlogPosts.Remove(post);
        await _context.SaveChangesAsync();
        return true;
    }
}