using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MusicSharing.Api.DTOs;

public class UpdateUserFormDto
{
    [Required] public string Username { get; set; } = default!;
    [Required][EmailAddress] public string Email { get; set; } = default!;
    public IFormFile? ProfilePicture { get; set; }
}