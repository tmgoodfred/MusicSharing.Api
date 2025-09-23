using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MusicSharing.Api.Data;
using MusicSharing.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MusicSharing.Api.Services
{
    public class UserService(AppDbContext context, ActivityService activityService)
    {
        private readonly AppDbContext _context = context;
        private readonly ActivityService _activityService = activityService;
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
            // Hash the password before saving
            user.PasswordHash = _passwordHasher.HashPassword(user, user.PasswordHash);
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

            // Persist profile picture path changes
            if (user.ProfilePicturePath != updatedUser.ProfilePicturePath)
            {
                user.ProfilePicturePath = updatedUser.ProfilePicturePath;
            }

            // Only hash and update password if it was changed
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

            // Delete all activity created by this user
            await _activityService.DeleteByUserAsync(id);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        // Add a method to verify passwords
        public bool VerifyPassword(User user, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<User?> AuthenticateAsync(string usernameOrEmail, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == usernameOrEmail || u.Email == usernameOrEmail);

            if (user == null)
                return null;

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

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
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
            var term = query.Trim().ToLower();

            return await _context.Users
                .Where(u => u.Username.ToLower().Contains(term) || u.Email.ToLower().Contains(term))
                .OrderBy(u => u.Username)
                .Take(take)
                .ToListAsync();
        }
    }
}