using Microsoft.AspNetCore.Mvc;
using MusicSharing.Api.DTOs;
using MusicSharing.Api.Services;

namespace MusicSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FollowerController(FollowerService followerService) : ControllerBase
{
    private readonly FollowerService _followerService = followerService;

    [HttpPost("follow")]
    public async Task<IActionResult> Follow([FromQuery] int followerId, [FromQuery] int followedId)
    {
        var result = await _followerService.FollowAsync(followerId, followedId);
        if (!result) return BadRequest("Already following or invalid.");
        return Ok();
    }

    [HttpPost("unfollow")]
    public async Task<IActionResult> Unfollow([FromQuery] int followerId, [FromQuery] int followedId)
    {
        var result = await _followerService.UnfollowAsync(followerId, followedId);
        if (!result) return NotFound();
        return Ok();
    }

    [HttpGet("{userId}/followers")]
    public async Task<IActionResult> GetFollowers(int userId)
    {
        var users = await _followerService.GetFollowersAsync(userId);
        var dtos = users.Select(u => new FollowerUserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email
        }).ToList();
        return Ok(dtos);
    }

    [HttpGet("{userId}/following")]
    public async Task<IActionResult> GetFollowing(int userId)
    {
        var users = await _followerService.GetFollowingAsync(userId);
        var dtos = users.Select(u => new FollowerUserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email
        }).ToList();
        return Ok(dtos);
    }
}