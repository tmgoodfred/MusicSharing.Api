using Microsoft.AspNetCore.Mvc;
using MusicSharing.Api.DTOs;
using MusicSharing.Api.Models;
using MusicSharing.Api.Services;

namespace MusicSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingController(RatingService ratingService) : ControllerBase
{
    private readonly RatingService _ratingService = ratingService;

    // GET: api/rating/song/{songId}
    [HttpGet("song/{songId}")]
    public async Task<IActionResult> GetBySong(int songId)
    {
        var ratings = await _ratingService.GetRatingsBySongAsync(songId);
        var avg = await _ratingService.GetAverageRatingAsync(songId);
        return Ok(new { ratings, average = avg });
    }

    // POST: api/rating
    [HttpPost]
    public async Task<IActionResult> AddOrUpdate([FromBody] CreateRatingDto dto)
    {
        var rating = new Rating
        {
            SongId = dto.SongId,
            UserId = dto.UserId,
            RatingValue = dto.RatingValue,
            CreatedAt = DateTime.UtcNow
        };
        var result = await _ratingService.AddOrUpdateRatingAsync(rating);
        return Ok(result);
    }

    // DELETE: api/rating/{ratingId}
    [HttpDelete("{ratingId}")]
    public async Task<IActionResult> Delete(int ratingId, [FromQuery] int? userId = null, [FromQuery] bool isAdmin = false)
    {
        var deleted = await _ratingService.DeleteRatingAsync(ratingId, userId, isAdmin);
        if (!deleted) return Forbid();
        return NoContent();
    }
}