using Galaxxis.Core.Provider;
using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Mvc;
using System;
using Models = Galaxxis.Data.Models;

namespace Galaxxis.Web.Views.Planet
{
    public class PlanetController : Controller
    {
        //ICurrentUserService _currentUserService;
        //public PlanetController(ICurrentUserService currentUserService)
        //{
        //    _currentUserService = currentUserService 
        //        ?? throw new ArgumentNullException(nameof(currentUserService));
        //}
        public IActionResult Index(int id = 0)
        {
            if (id <= 0)
            {
                return RedirectToAction("Index", "Home");
            }

            Models.Planet planet = PlanetProvider.GetPlanet(id);

            PlanetViewModel model = new PlanetViewModel
            {
                Planet = planet
            };

            return View("Planet", model);
        }
    }
}
