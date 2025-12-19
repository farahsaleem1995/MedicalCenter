using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Options;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// SMTP client implementation for sending emails.
/// Supports both production SMTP servers and MailDev for development.
/// </summary>
public class SmtpClient : ISmtpClient
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpClient> _logger;

    public SmtpClient(IOptions<SmtpOptions> options, ILogger<SmtpClient> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result> SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new System.Net.Mail.SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                // Skip credentials for MailDev (development mode - no authentication needed)
                Credentials = _options.UseMailDev 
                    ? null 
                    : new NetworkCredential(_options.Username, _options.Password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName),
                To = { new MailAddress(to) },
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await client.SendMailAsync(message, cancellationToken);
            
            // Log success with MailDev web UI link in development
            if (_options.UseMailDev)
            {
                _logger.LogInformation(
                    "Email sent via MailDev. View at: http://localhost:{Port}",
                    _options.MailDevWebPort);
            }
            else
            {
                _logger.LogInformation("Email sent to {To}", to);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return Result.Failure(new Error(ErrorCodes.InternalServerError, $"Failed to send email: {ex.Message}"));
        }
    }
}

