
namespace Galaxxis.Web.Code.Identity
{
    using Microsoft.AspNetCore.Identity;

    public class AppUser : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
