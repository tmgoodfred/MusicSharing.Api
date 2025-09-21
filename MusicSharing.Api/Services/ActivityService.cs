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
    }
}