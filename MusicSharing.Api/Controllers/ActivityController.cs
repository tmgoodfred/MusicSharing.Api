using Microsoft.AspNetCore.Mvc;
using MusicSharing.Api.DTOs;
using MusicSharing.Api.Models;
using MusicSharing.Api.Services;

namespace MusicSharing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController(ActivityService activityService) : ControllerBase
    {
        private readonly ActivityService _activityService = activityService;

        // GET: api/activity/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetForUser(int userId, [FromQuery] int count = 20)
        {
            var activities = await _activityService.GetForUserAsync(userId, count);
            var dtos = activities.Select(a => new ActivityDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Type = a.Type,
                Data = a.Data,
                CreatedAt = a.CreatedAt
            }).ToList();
            return Ok(dtos);
        }

        // GET: api/activity/feed/{userId}
        [HttpGet("feed/{userId}")]
        public async Task<IActionResult> GetFeed(int userId, [FromQuery] int count = 20)
        {
            var activities = await _activityService.GetRecentForUserAndFollowingAsync(userId, count);
            var dtos = activities.Select(a => new ActivityDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Type = a.Type,
                Data = a.Data,
                CreatedAt = a.CreatedAt
            }).ToList();
            return Ok(dtos);
        }

        // POST: api/activity
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ActivityDto dto)
        {
            var activity = new Activity
            {
                UserId = dto.UserId,
                Type = dto.Type,
                Data = dto.Data,
                CreatedAt = DateTime.UtcNow
            };
            var created = await _activityService.AddAsync(activity);
            return CreatedAtAction(nameof(GetForUser), new { userId = created.UserId }, dto);
        }
    }
}