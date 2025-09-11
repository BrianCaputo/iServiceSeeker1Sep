using iServiceSeeker1Sep.Data;
using Microsoft.AspNetCore.Identity;

namespace iServiceSeeker1Sep.Services
{
    public interface IUserTrackingService
    {
        Task TrackLoginAsync(ApplicationUser user, bool isLocalLogin = true);
        Task TrackPasswordSetAsync(ApplicationUser user);
    }

    public class UserTrackingService : IUserTrackingService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserTrackingService> _logger;

        public UserTrackingService(UserManager<ApplicationUser> userManager, ILogger<UserTrackingService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task TrackLoginAsync(ApplicationUser user, bool isLocalLogin = true)
        {
            try
            {
                user.LastLoginDate = DateTime.UtcNow;

                // If local login and no password flag set, update it
                if (isLocalLogin && !user.HasLocalPassword)
                {
                    user.HasLocalPassword = true;
                    user.LocalPasswordAddedAt ??= DateTime.UtcNow;
                }

                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Login tracked for user {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track login for user {Email}", user.Email);
            }
        }

        public async Task TrackPasswordSetAsync(ApplicationUser user)
        {
            try
            {
                user.HasLocalPassword = true;
                user.LocalPasswordAddedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Password set tracked for user {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track password set for user {Email}", user.Email);
            }
        }
    }
}