using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Models;

namespace MusicSharing.Api.Services;

public partial class UserService
{
    private static string HashSha256(string value)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    public async Task<string?> CreatePasswordResetTokenAsync(string usernameOrEmail, string? ip, string? userAgent, TimeSpan? lifetime = null)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);

        // Always return null style (avoid enumeration)
        if (user == null || !user.EmailConfirmed)
            return null;

        var now = DateTime.UtcNow;

        // Throttle: limit active requests
        var recentCount = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && t.CreatedAtUtc > now.AddHours(-1))
            .CountAsync();
        if (recentCount >= 5)
            return null;

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        var entity = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashSha256(rawToken),
            ExpiresAtUtc = now.Add(lifetime ?? TimeSpan.FromMinutes(30)),
            RequestIp = ip,
            UserAgent = userAgent
        };

        _context.PasswordResetTokens.Add(entity);
        await _context.SaveChangesAsync();
        return rawToken;
    }

    public async Task<bool> ResetPasswordAsync(string rawToken, string newPassword)
    {
        var hash = HashSha256(rawToken);
        var now = DateTime.UtcNow;

        var tokenEntity = await _context.PasswordResetTokens
            .Include(t => t.User)
            .Where(t => t.TokenHash == hash && t.UsedAtUtc == null && t.ExpiresAtUtc >= now)
            .FirstOrDefaultAsync();

        if (tokenEntity == null) return false;

        // Mark used first (race safety)
        tokenEntity.UsedAtUtc = now;

        var user = tokenEntity.User;
        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SendPasswordResetEmailAsync(int userId, string baseUrl, string rawToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || string.IsNullOrWhiteSpace(user.Email)) return;

        var link = $"{baseUrl.TrimEnd('/')}/reset-password?token={rawToken}";
        var html = $"""
            <p>Hello {System.Net.WebUtility.HtmlEncode(user.Username)},</p>
            <p>We received a request to reset your password.</p>
            <p><a href="{link}">Reset Password</a></p>
            <p>If you did not request this, you can ignore this email.</p>
            """;

        await _emailSender.SendAsync(user.Email, "Reset your password", html, $"Visit {link} to reset your password.");
    }
}