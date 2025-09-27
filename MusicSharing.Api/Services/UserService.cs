using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MusicSharing.Api.Data;
using MusicSharing.Api.Models;
using MusicSharing.Api.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MusicSharing.Api.Services
{
    public partial class UserService(AppDbContext context, ActivityService activityService, IEmailSender emailSender)
    {
        private readonly AppDbContext _context = context;
        private readonly ActivityService _activityService = activityService;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public string GenerateJwtToken(User user, IConfiguration config)
        {
            var jwtSettings = config.GetSection("Jwt");
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            // Check if email already exists (case-insensitive)
            var exists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == user.Email.ToLower());
            if (exists)
                throw new InvalidOperationException("A user with this email already exists.");

            user.PasswordHash = _passwordHasher.HashPassword(user, user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateUserAsync(int id, User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;
            user.ProfilePicturePath = updatedUser.ProfilePicturePath;

            if (!string.IsNullOrWhiteSpace(updatedUser.PasswordHash))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, updatedUser.PasswordHash);
            }

            user.Role = updatedUser.Role;

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            await _activityService.DeleteByUserAsync(id);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> GetUserByIdAsync(int id) => await _context.Users.FindAsync(id);

        public async Task<List<User>> GetAllUsersAsync() => await _context.Users.ToListAsync();

        public bool VerifyPassword(User user, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<User?> AuthenticateAsync(string usernameOrEmail, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);

            if (user == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success ? user : null;
        }

        public async Task<User?> UpdateUserProfileAsync(int id, string username, string email)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;
            user.Username = username;
            user.Email = email;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<string> SaveProfilePictureAsync(IFormFile file, string uploadFolder)
        {
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tif", ".tiff", ".svg", ".ico", ".heic", ".heif", ".avif" };

            var ext = Path.GetExtension(file.FileName);
            var contentType = file.ContentType ?? "";

            var looksLikeImage = contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                                 || (ext.Length > 0 && allowedExtensions.Contains(ext));

            if (!looksLikeImage)
                throw new InvalidOperationException("Unsupported image type.");

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }

        public async Task<List<User>> SearchUsersAsync(string query, int take = 20)
        {
            if (string.IsNullOrWhiteSpace(query)) return [];
            var pattern = $"%{query.Trim()}%";

            return await _context.Users
                .Where(u =>
                    EF.Functions.ILike(u.Username, pattern) ||
                    EF.Functions.ILike(u.Email, pattern))
                .OrderBy(u => u.Username)
                .Take(take)
                .ToListAsync();
        }
    }
}