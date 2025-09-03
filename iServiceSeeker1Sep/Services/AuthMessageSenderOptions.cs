namespace iServiceSeeker1Sep.Services;

public class AuthMessageSenderOptions
{
    public string? SendGridKey { get; set; } // Keep for backward compatibility

    // SMTP Configuration
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Your App Name";
}