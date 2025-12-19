namespace MedicalCenter.Infrastructure.Options;

/// <summary>
/// SMTP configuration options.
/// Supports both production SMTP servers and MailDev for development.
/// </summary>
public class SmtpOptions
{
    public const string SectionName = "Smtp";

    /// <summary>
    /// SMTP server host address.
    /// For MailDev: "localhost"
    /// For production: e.g., "smtp.gmail.com"
    /// </summary>
    public string Host { get; set; } = string.Empty;
    
    /// <summary>
    /// SMTP server port.
    /// For MailDev: 1025
    /// For production: typically 587 (TLS) or 465 (SSL)
    /// </summary>
    public int Port { get; set; } = 587;
    
    /// <summary>
    /// Enable SSL/TLS encryption.
    /// For MailDev: false
    /// For production: true
    /// </summary>
    public bool EnableSsl { get; set; } = true;
    
    /// <summary>
    /// SMTP username for authentication.
    /// For MailDev: not required (leave empty)
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// SMTP password for authentication.
    /// For MailDev: not required (leave empty)
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    
    /// <summary>
    /// When true, uses MailDev mode (no authentication required).
    /// Set to true in Development environment.
    /// </summary>
    public bool UseMailDev { get; set; } = false;
    
    /// <summary>
    /// MailDev Web UI port for viewing captured emails.
    /// Default: 1080
    /// </summary>
    public int MailDevWebPort { get; set; } = 1080;
}

