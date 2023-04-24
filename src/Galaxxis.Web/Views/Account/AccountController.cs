using Galaxxis.Web.Code.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Galaxxis.Web.Views.Account
{
    public class AccountController : Controller
    {
        private UserManager<AppUser> UserMgr { get; }

        private SignInManager<AppUser> SignInMgr { get; }

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) 
        {
            this.UserMgr = userManager;
            this.SignInMgr = signInManager;
        }

        public async Task<IActionResult> Login(string returnUrl = null, string user = null, string password = null)
        {
            if (user == null || password == null)
            {
                return View();
            }

            //Microsoft.AspNetCore.Identity.SignInResult result = await SignInMgr.PasswordSignInAsync("Administrator", "***REMOVED***", true, false);
            Microsoft.AspNetCore.Identity.SignInResult result = await SignInMgr
                .PasswordSignInAsync(user, password, true, false);
            if (result.Succeeded)
            {
                if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return Redirect(returnUrl);
                }
            }
            else
            {
                ViewBag.Message = "Login failed: " + result.ToString();
            }
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await SignInMgr.SignOutAsync();
            return RedirectToAction("Index", "Home");
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
