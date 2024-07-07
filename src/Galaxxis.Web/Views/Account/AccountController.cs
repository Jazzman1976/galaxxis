using Galaxxis.Web.Code.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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

        public async Task<IActionResult> Login(LoginViewModel model)
        {
            //"Administrator", "***REMOVED***"

            if (model.User == null || model.Password == null)
            {
                return View();
            }

            Microsoft.AspNetCore.Identity.SignInResult result = await SignInMgr
                .PasswordSignInAsync(model.User, model.Password, true, false);
            if (result.Succeeded)
            {
                if (string.IsNullOrWhiteSpace(model.ReturnUrl) || !Url.IsLocalUrl(model.ReturnUrl))
                {
                    return RedirectToAction("Index", "Start");
                }
                else
                {
                    return Redirect(model.ReturnUrl);
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

        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag.Message = TempData[nameof(AccountController) + nameof(Register)];

            if (model.IsEmpty())
            {
                return View();
            }

            if (model.IsIncomplete())
            {
                ViewBag.Message = "Please submit all fields!";
                return View(model);
            }

            if (!model.IsPasswordValid())
            {
                ViewBag.Message = "Pasword fields does not match! Check both password fields!";
                return View(model);
            }

            try
            {
                AppUser existingEmailUser = await UserMgr.FindByEmailAsync(model.Email);
                AppUser existingUsernameUser = await UserMgr.FindByNameAsync(model.Username);
                if (existingEmailUser == null && existingUsernameUser == null)
                {
                    AppUser newUser = new()
                    {
                        UserName = model.Username,
                        Email = model.Email,
                        FirstName = model.Firstname,
                        LastName = model.Lastname
                    };

                    IdentityResult result = await UserMgr.CreateAsync(newUser, model.Password);
                    if (result.Errors.Any()) 
                    {
                        string errors = string.Join("<br />", result.Errors.Select(error => error.Description));
                        throw new Exception(errors);
                    }
                    TempData[nameof(AccountController) + nameof(Register)] = "User has been created.";
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    ViewBag.Message = "User still exists";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(model);
            }
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
