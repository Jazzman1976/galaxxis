using Galaxxis.Core.Provider;
using Galaxxis.Core.Static;
using Galaxxis.Data.Models;
using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Numerics;
using Models = Galaxxis.Data.Models;

namespace Galaxxis.Web.Views.Planet
{
    [Authorize]
    public class PlanetController : Controller
    {
        ICurrentUserService _currentUserService;
        public PlanetController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public IActionResult Index(int id = 0)
        {
            User user = _currentUserService
                .GetGalaxxisUserAsync()
                .GetAwaiter()
                .GetResult();
            bool isValidPlanet = false;
            foreach (Models.Planet p in user.Planet)
            {
                if (p.PlanetId == id)
                {
                    isValidPlanet = true;
                    break;
                }
            }

            if (isValidPlanet)
            {
                Models.Planet planet = PlanetProvider.GetPlanet(id);
                PlanetViewModel model = new() { Planet = planet };

                System.Diagnostics.Debug.WriteLine(
                    $"{DateTime.Now.ToString(Constants.DEBUG_TIME_FORMAT)}: "
                    + $"Planet view opened for planet {planet.PlanetName}");

                return View("Planet", model);
            }

            return new EmptyResult();
        }
    }
}
