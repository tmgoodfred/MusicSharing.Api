using Microsoft.AspNetCore.Http;
using MusicSharing.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace MusicSharing.Api.DTOs;

public class CreateUserFormDto
{
    [Required] public string Username { get; set; } = default!;
    [Required][EmailAddress] public string Email { get; set; } = default!;
    [Required] public string PasswordHash { get; set; } = default!;
    [Required] public UserRole Role { get; set; } = UserRole.User;
    public IFormFile? ProfilePicture { get; set; }
}