using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Data;
using MusicSharing.Api.Models;

namespace MusicSharing.Api.Services
{
    public class ActivityService(AppDbContext context)
    {
        private readonly AppDbContext _context = context;

        public async Task<List<Activity>> GetRecentForUserAndFollowingAsync(int userId, int count = 20)
        {
            // Get IDs of users this user follows (excluding self)
            var followingIds = await _context.Followers
                .Where(f => f.FollowerUserId == userId)
                .Select(f => f.FollowedUserId)
                .ToListAsync();

            if (followingIds.Count == 0)
            {
                // No following: return global feed
                return await _context.Activities
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(count)
                    .ToListAsync();
            }

            followingIds.Add(userId); // include self

            return await _context.Activities
                .Where(a => followingIds.Contains(a.UserId))
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Activity>> GetForUserAsync(int userId, int count = 20)
        {
            return await _context.Activities
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Activity> AddAsync(Activity activity)
        {
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
            return activity;
        }

        // --- New helpers for cleanup ---

        public async Task<int> DeleteByUserAsync(int userId)
        {
            return await _context.Activities
                .Where(a => a.UserId == userId)
                .ExecuteDeleteAsync();
        }

        public async Task<int> DeleteBySongAsync(int songId)
        {
            // Match JSON payloads that contain "SongId":<id>
            return await _context.Activities
                .Where(a => a.Data != null && a.Data.Contains($"\"SongId\":{songId}"))
                .ExecuteDeleteAsync();
        }

        public async Task<int> DeleteByCommentAsync(int commentId)
        {
            // Match JSON payloads that contain "CommentId":<id>
            return await _context.Activities
                .Where(a => a.Data != null && a.Data.Contains($"\"CommentId\":{commentId}"))
                .ExecuteDeleteAsync();
        }

        public async Task<int> DeleteByBlogPostAsync(int blogPostId)
        {
            // Match JSON payloads that contain "BlogPostId":<id>
            return await _context.Activities
                .Where(a => a.Data != null && a.Data.Contains($"\"BlogPostId\":{blogPostId}"))
                .ExecuteDeleteAsync();
        }

        public async Task<List<Activity>> GetAllAsync(int? count = null)
        {
            IQueryable<Activity> query = _context.Activities.OrderByDescending(a => a.CreatedAt);
            if (count.HasValue)
                query = query.Take(count.Value);
            return await query.ToListAsync();
        }
    }
}