using MedicalCenter.Core.Primitives;

namespace MedicalCenter.Core.Services;

/// <summary>
/// Abstraction for sending emails via SMTP.
/// </summary>
public interface ISmtpClient
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML or plain text)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}

