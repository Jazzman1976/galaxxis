using Galaxxis.Core.Provider;
using Galaxxis.Data.Models;
using Galaxxis.Web.Code.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Galaxxis.Web.Code.Services.CurrentUserService
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<AppUser> _userManager;
        public CurrentUserService(
            IHttpContextAccessor contextAccessor,
            UserManager<AppUser> userManager)
        {
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        /// <summary>
        /// Get the current user's ASP.Net Identity ID from the HTTP context.<br/>
        /// It's not the Galaxxis ID.
        /// </summary>
        /// <returns>String identifier of the ASP.Net Identity user.</returns>
        public string GetAspNetUserId()
        {
            string userId = _contextAccessor?.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }

        /// <summary>
        /// Get the name if the current ASP.Net Identity user from the HTTP context.<br/>
        /// It's not the Galaxxis username.
        /// </summary>
        /// <returns>String name of the ASP.Net Identity user.</returns>
        public string GetAspNetUserName()
        {
            string userName = _contextAccessor?.HttpContext?.User?.Identity?.Name;
            return userName;
        }

        public async Task<AppUser> GetCurrentAspNetAppUserAsync()
        {
            var userId = GetAspNetUserId();
            if (userId == null)
            {
                return null;
            }

            AppUser user = await _userManager.FindByIdAsync(userId);


            return user;
        }

        public async Task<User> GetGalaxxisUserAsync()
        {
            AppUser currentUser = await GetCurrentAspNetAppUserAsync();
            if (currentUser == null)
            {
                return null;
            }

            // Get the Galaxxis user by email
            User galaxxisUser = UserProvider.GetUser(currentUser.Email, includeData: true);
            return galaxxisUser;
        }
    }
}
