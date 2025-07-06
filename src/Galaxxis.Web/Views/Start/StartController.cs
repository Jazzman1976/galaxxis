using Galaxxis.Core.Provider;
using Galaxxis.Data.Models;
using Galaxxis.Web.Code.Identity;
using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Galaxxis.Web.Views.Start
{
    public class StartController : Controller
    {
        ICurrentUserService _currentUserService;
        
        public StartController(ICurrentUserService currentUserService) 
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // get the current user
                AppUser CurrentUser = await _currentUserService.GetCurrentUserAsync();
                if (CurrentUser == null)
                {
                    return View("Start");
                }

                // get planets of user
                bool hasPlanets = PlanetProvider
                    .GetPlanetsOfUser(CurrentUser.Id)
                    .Any();
                if (hasPlanets)
                {
                    return RedirectToAction("Index", "Home");
                }

                // if user has no planets, redirect to new player page
                return RedirectToAction("Index", "NewPlayer");
            }

            // if user is not authenticated, redirect to start page
            return View("Start");
        }
    }
}
