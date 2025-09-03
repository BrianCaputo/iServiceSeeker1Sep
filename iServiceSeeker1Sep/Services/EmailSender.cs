using iServiceSeeker1Sep.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace iServiceSeeker1Sep.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;

    public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                       ILogger<EmailSender> logger)
    {
        Options = optionsAccessor.Value;
        _logger = logger;
    }

    public AuthMessageSenderOptions Options { get; }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(Options.SmtpServer))
        {
            throw new Exception("SMTP Server not configured");
        }

        if (string.IsNullOrEmpty(Options.SmtpUsername) || string.IsNullOrEmpty(Options.SmtpPassword))
        {
            throw new Exception("SMTP credentials not configured");
        }

        await ExecuteAsync(toEmail, subject, message);
    }

    private async Task ExecuteAsync(string toEmail, string subject, string message)
    {
        try
        {
            _logger.LogInformation($"Starting email send to {toEmail}");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(Options.FromName, Options.FromEmail));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message,
                TextBody = StripHtml(message) // Provide plain text fallback
            };
            email.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            // Connect with appropriate security options
            SecureSocketOptions secureSocketOptions;
            if (Options.SmtpPort == 465)
            {
                secureSocketOptions = SecureSocketOptions.SslOnConnect; // Implicit SSL
            }
            else if (Options.SmtpPort == 587)
            {
                secureSocketOptions = SecureSocketOptions.StartTls; // Explicit SSL
            }
            else
            {
                secureSocketOptions = Options.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            }

            await client.ConnectAsync(Options.SmtpServer, Options.SmtpPort, secureSocketOptions);

            // Authenticate
            await client.AuthenticateAsync(Options.SmtpUsername, Options.SmtpPassword);

            // Send the email
            await client.SendAsync(email);

            // Disconnect
            await client.DisconnectAsync(true);

            _logger.LogInformation($"Email to {toEmail} sent successfully via MailKit!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"MailKit error sending email to {toEmail}: {ex.Message}");
            throw;
        }
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        // Simple HTML stripping for plain text fallback
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
    }
}