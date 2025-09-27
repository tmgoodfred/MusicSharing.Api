using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Data;
using MusicSharing.Api.Models;

namespace MusicSharing.Api.Services
{
    public class AdminService
    {
        private readonly AppDbContext _context;
        private readonly ActivityService _activityService;
        public AdminService(AppDbContext context, ActivityService activityService)
        {
            _context = context;
            _activityService = activityService;
        }

        public async Task<object> GetDashboardAsync()
        {
            var users = await _context.Users.ToListAsync();
            var songs = await _context.Songs.Include(s => s.Categories).Include(s => s.Ratings).Include(s => s.Comments).ToListAsync();
            var comments = await _context.Comments.ToListAsync();
            var activities = await _context.Activities.ToListAsync();
            var blogs = await _context.BlogPosts.ToListAsync();
            return new
            {
                Users = users,
                Songs = songs,
                Comments = comments,
                Activities = activities,
                BlogPosts = blogs
            };
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            await _activityService.DeleteByUserAsync(userId);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSongAsync(int songId)
        {
            var song = await _context.Songs.FindAsync(songId);
            if (song == null) return false;
            await _activityService.DeleteBySongAsync(songId);
            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return false;
            await _activityService.DeleteByCommentAsync(commentId);
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBlogPostAsync(int blogId)
        {
            var blog = await _context.BlogPosts.FindAsync(blogId);
            if (blog == null) return false;
            await _activityService.DeleteByBlogPostAsync(blogId);
            _context.BlogPosts.Remove(blog);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PromoteUserToAdminAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            user.Role = UserRole.Admin;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
