﻿namespace MusicSharing.Api.Services.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null, CancellationToken ct = default);
}