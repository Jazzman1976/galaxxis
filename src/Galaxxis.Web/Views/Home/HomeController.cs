using Galaxxis.Core.Extensions.PlanetExtensions;
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
            HomeViewModel model
                   = new HomeViewModel();

            string info = null;
            Planet planet;            
            using (GalaxxisContext context = new GalaxxisContext())
            {
                planet = context.Planet.FirstOrDefault(p => p.PlanetId == planetId);
                if (planet != null)
                {
                    planet.UpdateRessources();
                    
                    if (planet.OreMine_BuildOreMine(out info))
                    {
                        context.SaveChanges();
                    }
                }
                else
                {
                    info = "Planet not found!";
                }
            }
            model.Info = info;

            return View("Home", model);
        }

        [HttpPost]
        public IActionResult BuildMageAcademy(int planetId)
        {
            HomeViewModel model = new HomeViewModel();

            string info = null;
            Planet planet;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                planet = context.Planet.FirstOrDefault(p => p.PlanetId == planetId);
                if (planet != null)
                {
                    planet.UpdateRessources();

                    if (planet.MageAcademy_Build(out info))
                    {
                        context.SaveChanges();
                    }
                }
                else
                {
                    info = "Planet not found!";
                }
            }
            model.Info = info;

            return View("Home", model);
        }
    }
}
