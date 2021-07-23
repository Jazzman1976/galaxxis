using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Classes.Buildings.PlanetBuildings;
using Galaxxis.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Home
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // exit
            return View("Home", new HomeViewModel());
        }

        [HttpPost]
        public IActionResult BuildOreMine(int planetId)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId);

            // build ore mine
            OreMine oreMine = new OreMine(planet);       
            oreMine.Build(out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }

        [HttpPost]
        public IActionResult BuildMageAcademy(int planetId)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId);

            // build mage academy
            MageAcademy mageAcademy = new MageAcademy(planet);
            mageAcademy.Build(out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }

        [HttpPost]
        public IActionResult BuildPowerSupply(int planetId)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId);

            // build mage power supply
            PowerSupply powerSupply = new PowerSupply(planet);
            powerSupply.Build(out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }

        [HttpPost]
        public IActionResult BuildFactory(int planetId)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId);

            // build factory
            Factory factory = new Factory(planet);
            factory.Build(out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }

        [HttpPost]
        public IActionResult BuildCrystalMine(int planetId)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId);

            // build chrystal mine
            CrystalMine chrystalMine = new CrystalMine(planet);
            chrystalMine.Build(out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }
    }
}
