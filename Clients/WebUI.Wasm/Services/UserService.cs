using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebUI_Wasm.Data;

namespace WebUI_Wasm.Services
{
    [Authorize]
    public class UserService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpContext _httpContext;
        private ApplicationUser? CurrentUser { get; set; }
        public UserService(AuthenticationStateProvider authenticationStateProvider, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _userManager = userManager;
            _httpContext = httpContextAccessor.HttpContext;
        }

        /// <summary>
        /// Current User Name
        /// </summary>
        public string? UserName => _httpContext.User.Identity?.Name;
        /// <summary>
        /// Current User Id
        /// </summary>
        public string? UserId => userId ??= _userManager.GetUserId(_httpContext.User);
        string? userId;
        /// <summary>
        /// Current User Is Admin?
        /// </summary>
        public bool IsAdmin => isAdmin ??= _httpContext.User.IsInRole(RoleConsts.Admin);
        bool? isAdmin;

        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            if (CurrentUser == null)
            {
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;
                if (user.Identity != null && user.Identity.IsAuthenticated)
                {
                    CurrentUser = await _userManager.GetUserAsync(user);
                }
            }
            return CurrentUser;
        }

        public async Task<bool> IsAdminAsync(ClaimsPrincipal? user = null)
        {
            if (user == null)
            {
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                user = authState.User;
            }
            var identityUser = await _userManager.GetUserAsync(user);
            return await _userManager.IsInRoleAsync(identityUser, RoleConsts.Admin);
        }
    }
}
