using Microsoft.AspNetCore.Identity;
using iServiceSeeker1Sep.Data;

namespace iServiceSeeker1Sep.Services
{
    /// <summary>
    /// Handles email confirmation logic for identity linking scenarios
    /// </summary>
    public class IdentityLinkingUserConfirmation : IUserConfirmation<ApplicationUser>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<IdentityLinkingUserConfirmation> _logger;

        public IdentityLinkingUserConfirmation(
            UserManager<ApplicationUser> userManager,
            ILogger<IdentityLinkingUserConfirmation> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<bool> IsConfirmedAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            try
            {
                // Rule 1: External primary auth users are automatically confirmed
                if (user.IsExternalPrimary)
                {
                    _logger.LogDebug("User {UserId} confirmed via external primary auth: {Method}",
                        user.Id, user.PrimaryAuthMethod);
                    return true;
                }

                // Rule 2: Local primary auth users need email confirmation for initial registration
                if (user.PrimaryAuthMethod == AuthenticationMethod.Local)
                {
                    var isEmailConfirmed = await manager.IsEmailConfirmedAsync(user);

                    if (!isEmailConfirmed)
                    {
                        _logger.LogDebug("User {UserId} requires email confirmation for local auth", user.Id);
                        return false;
                    }

                    // Mark initial confirmation as complete
                    if (!user.InitialEmailConfirmed)
                    {
                        user.InitialEmailConfirmed = true;
                        await manager.UpdateAsync(user);
                        _logger.LogInformation("Marked initial email confirmation complete for user {UserId}", user.Id);
                    }

                    return true;
                }

                // Fallback - shouldn't reach here
                _logger.LogWarning("Unexpected auth method for user {UserId}: {Method}",
                    user.Id, user.PrimaryAuthMethod);
                return await manager.IsEmailConfirmedAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking confirmation for user {UserId}", user.Id);
                // Fail safe - require confirmation on error
                return false;
            }
        }
    }

    /// <summary>
    /// Service for managing auth method linking operations
    /// </summary>
    public class AuthMethodLinkingService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthMethodLinkingService> _logger;

        public AuthMethodLinkingService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AuthMethodLinkingService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IdentityResult> AddLocalPasswordAsync(ApplicationUser user, string password)
        {
            if (user.HasLocalPassword)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicatePassword",
                    Description = "User already has a local password."
                });
            }

            var result = await _userManager.AddPasswordAsync(user, password);

            if (result.Succeeded)
            {
                user.HasLocalPassword = true;
                user.LocalPasswordAddedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Local password added for user {UserId}", user.Id);
            }

            return result;
        }

        public async Task<IdentityResult> LinkExternalAccountAsync(ApplicationUser user, ExternalLoginInfo info)
        {
            // Check if this external account is already linked to another user
            var existingUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "ExternalAccountAlreadyLinked",
                    Description = $"This {info.ProviderDisplayName} account is already linked to another user."
                });
            }

            var result = await _userManager.AddLoginAsync(user, info);

            if (result.Succeeded)
            {
                _logger.LogInformation("External login {Provider} linked for user {UserId}",
                    info.LoginProvider, user.Id);
            }

            return result;
        }

        public async Task<IdentityResult> UnlinkExternalAccountAsync(ApplicationUser user, string loginProvider, string providerKey)
        {
            // Safety check - don't allow unlinking if it's the only auth method
            if (!user.HasLocalPassword && user.UserLogins.Count <= 1)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "CannotRemoveLastAuthMethod",
                    Description = "Cannot remove the last authentication method. Add a password or another external account first."
                });
            }

            var result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);

            if (result.Succeeded)
            {
                _logger.LogInformation("External login {Provider} unlinked for user {UserId}",
                    loginProvider, user.Id);
            }

            return result;
        }
    }
}