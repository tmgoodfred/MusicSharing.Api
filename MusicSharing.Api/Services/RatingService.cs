using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Data;
using MusicSharing.Api.Models;

namespace MusicSharing.Api.Services;

public class RatingService
{
    private readonly AppDbContext _context;
    private readonly ActivityService _activityService;

    public RatingService(AppDbContext context, ActivityService activityService)
    {
        _context = context;
        _activityService = activityService;
    }

    public async Task<List<Rating>> GetRatingsBySongAsync(int songId)
    {
        return await _context.Ratings
            .Include(r => r.User)
            .Where(r => r.SongId == songId)
            .ToListAsync();
    }

    public async Task<double> GetAverageRatingAsync(int songId)
    {
        return await _context.Ratings
            .Where(r => r.SongId == songId)
            .AverageAsync(r => (double?)r.RatingValue) ?? 0.0;
    }

    public async Task<Rating> AddOrUpdateRatingAsync(Rating rating)
    {
        var existing = await _context.Ratings
            .FirstOrDefaultAsync(r => r.SongId == rating.SongId && r.UserId == rating.UserId);

        if (existing != null)
        {
            existing.RatingValue = rating.RatingValue;
            existing.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.Ratings.Add(rating);
        }

        await _context.SaveChangesAsync();

        // Log activity
        await _activityService.AddAsync(new Activity
        {
            UserId = rating.UserId,
            Type = "Rating",
            Data = $"{{\"SongId\":{rating.SongId},\"RatingValue\":{rating.RatingValue}}}",
            CreatedAt = DateTime.UtcNow
        });

        return existing ?? rating;
    }

    public async Task<bool> DeleteRatingAsync(int ratingId, int? userId = null, bool isAdmin = false)
    {
        var rating = await _context.Ratings.FindAsync(ratingId);
        if (rating == null) return false;

        // Only allow delete if admin or rating owner
        if (!isAdmin && rating.UserId != userId) return false;

        _context.Ratings.Remove(rating);
        await _context.SaveChangesAsync();
        return true;
    }
}