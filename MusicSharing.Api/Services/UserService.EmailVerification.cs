using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Models;
using MusicSharing.Api.Services.Interfaces;

namespace MusicSharing.Api.Services;

public partial class UserService
{
    // Removed duplicate _emailSender field (now declared in main file)

    private static string Sha256(string input)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
    }

    public async Task<string?> CreateEmailVerificationTokenAsync(int userId, string? ip, TimeSpan? lifetime = null)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || user.EmailConfirmed) return null;

        var now = DateTime.UtcNow;
        var recent = await _context.EmailVerificationTokens
            .Where(t => t.UserId == userId && t.CreatedAtUtc > now.AddHours(-1))
            .CountAsync();
        if (recent >= 3) return null;

        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        var entity = new EmailVerificationToken
        {
            UserId = userId,
            TokenHash = Sha256(raw),
            ExpiresAtUtc = now.Add(lifetime ?? TimeSpan.FromHours(4)),
            RequestIp = ip
        };

        _context.EmailVerificationTokens.Add(entity);
        await _context.SaveChangesAsync();
        return raw;
    }

    public async Task<bool> VerifyEmailAsync(string rawToken)
    {
        var hash = Sha256(rawToken);
        var now = DateTime.UtcNow;
        var token = await _context.EmailVerificationTokens
            .Include(t => t.User)
            .Where(t => t.TokenHash == hash && t.UsedAtUtc == null && t.ExpiresAtUtc >= now)
            .FirstOrDefaultAsync();

        if (token == null) return false;

        token.UsedAtUtc = now;
        token.User.EmailConfirmed = true;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SendVerificationEmailAsync(int userId, string baseUrl, string rawToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return;

        var link = $"{baseUrl.TrimEnd('/')}/verify-email?token={rawToken}";
        var html = $"""
            <p>Hello {System.Net.WebUtility.HtmlEncode(user.Username)},</p>
            <p>Please verify your email by clicking the link below:</p>
            <p><a href="{link}">Verify Email</a></p>
            <p>If you did not create an account, you can ignore this email.</p>
            """;

        await _emailSender.SendAsync(user.Email, "Verify your email", html, $"Visit {link} to verify your email.");
    }
}