using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Data;
using MusicSharing.Api.DTOs;
using MusicSharing.Api.Models;

namespace MusicSharing.Api.Services
{
    public class AdminService(AppDbContext context, ActivityService activityService)
    {
        private readonly AppDbContext _context = context;
        private readonly ActivityService _activityService = activityService;

        public async Task<object> GetDashboardAsync()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.Id)
                .Select(u => new UserProfileDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    CreatedAt = u.CreatedAt,
                    ProfilePicturePath = u.ProfilePicturePath,
                    EmailConfirmed = u.EmailConfirmed
                })
                .ToListAsync();

            var songs = await _context.Songs
                .Include(s => s.Categories)
                .Include(s => s.Ratings)
                .Include(s => s.Comments)
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            var comments = await _context.Comments
                .Include(c => c.User)
                .OrderByDescending(c => c.Id)
                .Select(c => new AdminCommentDto
                {
                    Id = c.Id,
                    SongId = c.SongId,
                    BlogPostId = c.BlogPostId,
                    CommentText = c.CommentText,
                    IsAnonymous = c.IsAnonymous,
                    CreatedAt = c.CreatedAt,
                    UserId = c.UserId,
                    Username = c.IsAnonymous ? null : c.User != null ? c.User.Username : null
                })
                .ToListAsync();

            var activities = await _context.Activities
                .OrderByDescending(a => a.Id)
                .ToListAsync();

            var blogs = await _context.BlogPosts
                .OrderByDescending(b => b.Id)
                .ToListAsync();

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
