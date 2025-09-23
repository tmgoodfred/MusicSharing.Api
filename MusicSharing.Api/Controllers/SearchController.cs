using Microsoft.AspNetCore.Mvc;
using MusicSharing.Api.DTOs;
using MusicSharing.Api.Models;
using MusicSharing.Api.Services;
using MusicSharing.Api.Services.Interfaces;

namespace MusicSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController(UserService userService, IMusicService musicService) : ControllerBase
{
    private readonly UserService _userService = userService;
    private readonly IMusicService _musicService = musicService;

    // GET: api/search?q=BigMax&take=20
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int take = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new GlobalSearchResultDto());

        try
        {
            take = Math.Clamp(take, 1, 50);

            // Users (uses ILike)
            var users = await _userService.SearchUsersAsync(q, take);

            // One consolidated song search (title OR artist OR uploader OR genre OR tag-exact)
            var songs = await _musicService.AdvancedSearchAsync(
                title: q,
                artist: q,
                genre: q,
                minPlays: null, maxPlays: null,
                minRating: null, maxRating: null,
                fromDate: null, toDate: null,
                tags: new List<string> { q },  // exact tag match
                categoryIds: null,
                uploader: q
            );

            var songDtos = songs
                .OrderByDescending(s => s.UploadDate)
                .Take(take)
                .Select(s => new SongSearchResultDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Artist = s.Artist,
                    Genre = s.Genre,
                    Tags = s.Tags,
                    UploadDate = s.UploadDate,
                    PlayCount = s.PlayCount,
                    DownloadCount = s.DownloadCount,
                    Uploader = s.User == null ? null : new UserProfileDto
                    {
                        Id = s.User.Id,
                        Username = s.User.Username,
                        Email = s.User.Email,
                        Role = s.User.Role.ToString(),
                        CreatedAt = s.User.CreatedAt,
                        ProfilePicturePath = s.User.ProfilePicturePath
                    }
                })
                .ToList();

            var userDtos = users.Select(u => new UserProfileDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt,
                ProfilePicturePath = u.ProfilePicturePath
            }).ToList();

            return Ok(new GlobalSearchResultDto { Users = userDtos, Songs = songDtos });
        }
        catch
        {
            return Problem("Search failed. Please try again.");
        }
    }
}