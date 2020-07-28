
namespace Galaxxis.Web.Controllers
{
    using Galaxxis.Web.Identity;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;

    public class AccountController : Controller
    {
        private UserManager<AppUser> UserMgr { get; }

        private SignInManager<AppUser> SignInMgr { get; }

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            this.UserMgr = userManager;
            this.SignInMgr = signInManager;
        }

        public IActionResult Login()
        {
            return new EmptyResult();
        }

        public IActionResult Logout()
        {
            return new EmptyResult();
        }

        public async Task<IActionResult> AddAdmin()
        {
            try
            {
                ViewBag.Message = "Admin still exists";

                // find or create admin account
                string email = "admin@example.com";
                AppUser user = await UserMgr.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new AppUser
                    {
                        UserName = "Administrator",
                        Email = email,
                        FirstName = "Elmar",
                        LastName = "Höfinghoff"
                    };

                    IdentityResult result = await UserMgr.CreateAsync(user, "***REMOVED***");
                    ViewBag.Message = "Admin user was created. UserName: Administrator, Password: ***REMOVED***";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            return View();
        }
    }
}
