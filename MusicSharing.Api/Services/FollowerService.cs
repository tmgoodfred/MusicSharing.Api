using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Data;
using MusicSharing.Api.Models;

namespace MusicSharing.Api.Services;

public class FollowerService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<bool> FollowAsync(int followerId, int followedId)
    {
        if (followerId == followedId) return false;
        if (await _context.Followers.AnyAsync(f => f.FollowerUserId == followerId && f.FollowedUserId == followedId))
            return false;

        _context.Followers.Add(new Follower { FollowerUserId = followerId, FollowedUserId = followedId });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnfollowAsync(int followerId, int followedId)
    {
        var rel = await _context.Followers
            .FirstOrDefaultAsync(f => f.FollowerUserId == followerId && f.FollowedUserId == followedId);
        if (rel == null) return false;
        _context.Followers.Remove(rel);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<User>> GetFollowersAsync(int userId)
    {
        return await _context.Followers
            .Where(f => f.FollowedUserId == userId)
            .Select(f => f.FollowerUser!)
            .ToListAsync();
    }

    public async Task<List<User>> GetFollowingAsync(int userId)
    {
        return await _context.Followers
            .Where(f => f.FollowerUserId == userId)
            .Select(f => f.FollowedUser!)
            .ToListAsync();
    }
}