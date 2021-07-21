using Galaxxis.Core.Classes.Buildings.PlanetBuildings;
using Galaxxis.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Galaxxis.Web.Views.Home
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            HomeViewModel model 
                = new HomeViewModel();

            return View("Home", model);
        }

        [HttpPost]
        public IActionResult BuildOreMine(int planetId)
        {
            // init view model
            HomeViewModel model = new HomeViewModel();

            // get planet from database
            Planet planet;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                planet = context.Planet.FirstOrDefault(p => p.PlanetId == planetId);
            }

            // build ore mine
            OreMine oreMine = new OreMine(planet);
            string info = null;            
            oreMine.Build(out info);

            // update view model
            model.Info = info;

            // show view
            return View("Home", model);
        }

        [HttpPost]
        public IActionResult BuildMageAcademy(int planetId)
        {
            HomeViewModel model = new HomeViewModel();

            // get planet from database
            Planet planet;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                planet = context.Planet.FirstOrDefault(p => p.PlanetId == planetId);
            }

            // build mage academy
            MageAcademy mageAcademy = new MageAcademy(planet);
            string info = null;
            mageAcademy.Build(out info);

            // update view model
            model.Info = info;

            return View("Home", model);
        }
    }
}
