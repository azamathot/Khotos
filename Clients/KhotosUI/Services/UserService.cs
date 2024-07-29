using KhotosUI.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace KhotosUI.Services
{
    public class UserService
    {
        private AuthenticationStateProvider _authStateProvider;
        public UserService(AuthenticationStateProvider authenticationStateProvider, IConfiguration configuration)
        {
            _authStateProvider = authenticationStateProvider;
            UpdateCurrentUserAsync().Wait();
            authenticationStateProvider.AuthenticationStateChanged += AuthenticationStateChanged;
        }
        private async void AuthenticationStateChanged(Task<AuthenticationState> task)
        {
            var authState = await task;
            if (authState.User.Identity.IsAuthenticated)
            {
                await UpdateCurrentUserAsync();
            }
            else
                UserId = UserName = string.Empty;
        }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public bool IsAdmin => User.IsInRole(RoleConsts.Admin);

        public ClaimsPrincipal User { get; set; }
        private async Task UpdateCurrentUserAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            User = authState.User;
            if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.FindFirstValue(JwtRegisteredClaimNames.PreferredUsername) != null)
                {
                    UserName = User.FindFirstValue(JwtRegisteredClaimNames.PreferredUsername)!;
                }
                if (User.FindFirst(JwtRegisteredClaimNames.Sub) != null)
                {
                    UserId = User.FindFirst(JwtRegisteredClaimNames.Sub).Value;
                }
            }
        }

        [Authorize]
        public IEnumerable<string> GetRoles()
        {
            return User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        }

    }
}
