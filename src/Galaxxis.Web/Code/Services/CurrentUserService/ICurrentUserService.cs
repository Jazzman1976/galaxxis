using Galaxxis.Data.Models;
using Galaxxis.Web.Code.Identity;
using System.Threading.Tasks;

namespace Galaxxis.Web.Code.Services.CurrentUserService
{
    public interface ICurrentUserService
    {
        string GetAspNetUserId();
        string GetAspNetUserName();
        Task<AppUser> GetCurrentAspNetAppUserAsync();
        Task<User> GetGalaxxisUserAsync();
    }
}
