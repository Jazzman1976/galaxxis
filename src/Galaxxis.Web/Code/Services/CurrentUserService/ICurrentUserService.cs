using Galaxxis.Web.Code.Identity;
using System.Threading.Tasks;

namespace Galaxxis.Web.Code.Services.CurrentUserService
{
    public interface ICurrentUserService
    {
        string GetUserId();
        string GetUserName();
        Task<AppUser> GetCurrentUserAsync();
    }
}
