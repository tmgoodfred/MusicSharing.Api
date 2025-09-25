using System.Net;
using System.Net.Mail;
using MusicSharing.Api.Services.Interfaces;

namespace MusicSharing.Api.Services;

public class SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly IConfiguration _config = config;
    private readonly ILogger<SmtpEmailSender> _logger = logger;

    public async Task SendAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null, CancellationToken ct = default)
    {
        var section = _config.GetSection("Email:Smtp");
        var host = section["Host"] ?? throw new InvalidOperationException("SMTP host missing");
        var port = int.Parse(section["Port"] ?? "587");
        var from = section["From"] ?? throw new InvalidOperationException("SMTP from missing");
        var user = section["User"];
        var pass = section["Pass"];
        var enableSsl = bool.Parse(section["EnableSsl"] ?? "true");

        using var msg = new MailMessage
        {
            From = new MailAddress(from),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        if (!string.IsNullOrEmpty(plainTextBody))
        {
            var altView = AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain");
            msg.AlternateViews.Add(altView);
        }
        msg.To.Add(toEmail);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrEmpty(user))
        {
            client.Credentials = new NetworkCredential(user, pass);
        }

        try
        {
            await client.SendMailAsync(msg, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }
}