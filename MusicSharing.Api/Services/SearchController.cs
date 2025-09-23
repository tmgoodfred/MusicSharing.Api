using Microsoft.AspNetCore.Mvc;
using MusicSharing.Api.DTOs;
using MusicSharing.Api.Models;
using MusicSharing.Api.Services;
using MusicSharing.Api.Services.Interfaces;
using System.Collections.Generic;

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

        take = Math.Clamp(take, 1, 50);

        // Users can be fetched concurrently
        var usersTask = _userService.SearchUsersAsync(q, take);

        // Songs: run sequentially to avoid DbContext concurrency exceptions
        var songsByTitle = await _musicService.AdvancedSearchAsync(
            title: q, artist: null, genre: null,
            minPlays: null, maxPlays: null,
            minRating: null, maxRating: null,
            fromDate: null, toDate: null,
            tags: null, categoryIds: null,
            uploader: null
        );

        var songsByArtist = await _musicService.AdvancedSearchAsync(
            title: null, artist: q, genre: null,
            minPlays: null, maxPlays: null,
            minRating: null, maxRating: null,
            fromDate: null, toDate: null,
            tags: null, categoryIds: null,
            uploader: null
        );

        var songsByUploader = await _musicService.AdvancedSearchAsync(
            title: null, artist: null, genre: null,
            minPlays: null, maxPlays: null,
            minRating: null, maxRating: null,
            fromDate: null, toDate: null,
            tags: null, categoryIds: null,
            uploader: q
        );

        // Deduplicate by Id, then limit
        var songLookup = new Dictionary<int, Song>();
        foreach (var s in songsByTitle) songLookup.TryAdd(s.Id, s);
        foreach (var s in songsByArtist) songLookup.TryAdd(s.Id, s);
        foreach (var s in songsByUploader) songLookup.TryAdd(s.Id, s);

        var songs = songLookup.Values
            .OrderByDescending(s => s.UploadDate)
            .Take(take)
            .ToList();

        var songDtos = songs.Select(s => new SongSearchResultDto
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
        }).ToList();

        var users = await usersTask;
        var userDtos = users.Select(u => new UserProfileDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role.ToString(),
            CreatedAt = u.CreatedAt,
            ProfilePicturePath = u.ProfilePicturePath
        }).ToList();

        return Ok(new GlobalSearchResultDto
        {
            Users = userDtos,
            Songs = songDtos
        });
    }
}