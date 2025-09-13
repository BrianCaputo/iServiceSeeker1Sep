using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ServiceSeeker.Services
{    public class EmailSenderAdapter<TUser> : IEmailSender<TUser>
        where TUser : class
    {
        private readonly IEmailSender _emailSender;

        public EmailSenderAdapter(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
        {
            return _emailSender.SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
        }

        public Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
        {
            return _emailSender.SendEmailAsync(email, "Reset your password",
                $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
        }

        public Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
        {
            return _emailSender.SendEmailAsync(email, "Reset your password",
                $"Please reset your password using the following code: {resetCode}");
        }
    }
}
