using ServiceSeeker.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ServiceSeeker.Services
{
    public class CustomSignInManager : SignInManager<ApplicationUser>
    {
        private readonly IUserTrackingService _trackingService;

        public CustomSignInManager(
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ApplicationUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<ApplicationUser> confirmation,
            IUserTrackingService trackingService)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            _trackingService = trackingService;
        }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var result = await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);

            if (result.Succeeded)
            {
                var user = await UserManager.FindByNameAsync(userName);
                if (user != null)
                {
                    await _trackingService.TrackLoginAsync(user, isLocalLogin: true);
                }
            }

            return result;
        }

        public override async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor)
        {
            var result = await base.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor);

            if (result.Succeeded)
            {
                var user = await UserManager.FindByLoginAsync(loginProvider, providerKey);
                if (user != null)
                {
                    await _trackingService.TrackLoginAsync(user, isLocalLogin: false);
                }
            }

            return result;
        }
    }
}