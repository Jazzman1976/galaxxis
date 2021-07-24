using Galaxxis.Core.Business.Provider;
using Galaxxis.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Galaxxis.Core.Extensions.PlanetExtensions;

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
            Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build ore mine
            planet.GetOreMine().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }

        [HttpPost]
        public IActionResult ChangeOreMineUsage(int planetId, double newValue)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // change ore mine usage
            planet.GetOreMine().ChangeUsage(newValue/100, out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }

        [HttpPost]
        public IActionResult BuildPowerSupply(int planetId)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build mage power supply
            planet.GetPowerSupply().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }

        [HttpPost]
        public IActionResult ChangePowerSupplyUsage(int planetId, double newValue)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // change ore mine usage
            planet.GetPowerSupply().ChangeUsage(newValue / 100, out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }

        [HttpPost]
        public IActionResult BuildFactory(int planetId)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build factory
            planet.GetFactory().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }

        [HttpPost]
        public IActionResult ChangeFactoryUsage(int planetId, double newValue)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // change ore mine usage
            planet.GetFactory().ChangeUsage(newValue / 100, out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }

        [HttpPost]
        public IActionResult BuildMageAcademy(int planetId)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build mage academy
            planet.GetMageAcademy().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }

        [HttpPost]
        public IActionResult ChangeMageAcademyUsage(int planetId, double newValue)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // change ore mine usage
            planet.GetMageAcademy().ChangeUsage(newValue / 100, out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }

        [HttpPost]
        public IActionResult BuildCrystalMine(int planetId)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build chrystal mine
            planet.GetCrystalMine().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }

        [HttpPost]
        public IActionResult BuildNeutronAccelerator(int planetId)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build Neutron Accelerator
            planet.GetNeutronAccelerator().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }

        [HttpPost]
        public IActionResult BuildElementiumSynthesizer(int planetId)
        {
            // get planet from database
            Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build Elementium Synthesizer
            planet.GetElementiumSynthesizer().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(info));
        }
    }
}
