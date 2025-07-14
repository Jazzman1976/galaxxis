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
using Models = Galaxxis.Data.Models;

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

                // get galaxxis user
                User player = UserProvider.GetUser(CurrentUser.Email, includeData: true);

                // get planets of user
                List<Models.Planet> planets = PlanetProvider.GetPlanetsOfUser(player.UserId);
                
                if (planets.Any())
                {
                    //return RedirectToAction("Index", "Home");

                    //TODO: add current location of user, which is a planet, to database
                    // User has a "Home", which is the first planet he started on.
                    // User starts each game on his home planet.
                    // User can look at other planets and do actions on them.

                    return RedirectToAction(
                        "Index", 
                        "Planet", 
                        new { id = planets.FirstOrDefault()?.PlanetId ?? 0 });
                }

                // if user has no planets, redirect to new player page
                return RedirectToAction("Index", "NewPlayer");
            }

            // if user is not authenticated, redirect to start page
            return View("Start");
        }
    }
}
