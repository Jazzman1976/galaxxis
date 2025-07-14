using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Core.Provider;
using Galaxxis.Data.Models;
using Galaxxis.Web.Code.Identity;
using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models = Galaxxis.Data.Models;

namespace Galaxxis.Web.Views.Home
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ICurrentUserService _currentUserService;

        public AppUser CurrentUser { get; private set; }

        private async Task InitializeAsync()
        {
            this.CurrentUser = await _currentUserService.GetCurrentUserAsync();
        }

        public HomeController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
            Task.Run(() => InitializeAsync()).Wait();
        }

        public IActionResult Index()
        {
            if (CurrentUser == null)
            {
                return RedirectToAction("Index", "Start");
            }

            

            // TODO: instead of home, go to the current location which is a planet
            // TODO: add id of current planet to the user data model and database

            HomeViewModel model;
            try
            {
                model = new HomeViewModel(CurrentUser);
            }
            catch
            {
                return RedirectToAction("Logout", "Account");
            }

            return View(
                "Home", 
                model);
        }

        [HttpPost]
        public IActionResult BuildOreMine(int planetId)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build ore mine
            planet.GetOreMine().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult ChangeOreMineUsage(int planetId, double newValue)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // change ore mine usage
            planet.GetOreMine().ChangeUsage(newValue/100, out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult BuildPowerSupply(int planetId)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build mage power supply
            planet.GetPowerSupply().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult ChangePowerSupplyUsage(int planetId, double newValue)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // change ore mine usage
            planet.GetPowerSupply().ChangeUsage(newValue / 100, out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult BuildFactory(int planetId)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build factory
            planet.GetFactory().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult ChangeFactoryUsage(int planetId, double newValue)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // change ore mine usage
            planet.GetFactory().ChangeUsage(newValue / 100, out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult BuildMageAcademy(int planetId)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build mage academy
            planet.GetMageAcademy().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult ChangeMageAcademyUsage(int planetId, double newValue)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // change ore mine usage
            planet.GetMageAcademy().ChangeUsage(newValue / 100, out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult BuildCrystalMine(int planetId)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build chrystal mine
            planet.GetCrystalMine().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult ChangeCrystalMineUsage(int planetId, double newValue)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // change ore mine usage
            planet.GetCrystalMine().ChangeUsage(newValue / 100, out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult BuildNeutronAccelerator(int planetId)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build Neutron Accelerator
            planet.GetNeutronAccelerator().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult ChangeNeutronAcceleratorUsage(int planetId, double newValue)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // change ore mine usage
            planet.GetNeutronAccelerator().ChangeUsage(newValue / 100, out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult BuildElementiumSynthesizer(int planetId)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build Elementium Synthesizer
            planet.GetElementiumSynthesizer().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult ChangeElementiumSynthesizerUsage(int planetId, double newValue)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // change ore mine usage
            planet.GetElementiumSynthesizer().ChangeUsage(newValue / 100, out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult BuildShipyard(int planetId)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build Shipyard
            planet.GetShipyard().Build(out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult BuildDefenseSystem(int planetId)
        {
            // get planet from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId, true);

            // build Defense System
            planet.GetDefenseSystem().Build(out string info);
            
            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult ProduceShipExplorer(int planetId, int userId)
        {
            // get entities from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId);

            // produce ship: explorer
            planet.GetShipyard().ProduceShipExplorer(out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult ProduceShipCarrier(int planetId, int userId)
        {
            // get entities from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId);

            // produce ship: explorer
            planet.GetShipyard().ProduceShipCarrier(out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult ProduceDefenseLightManualCannon(int planetId)
        {
            // get entities from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId);

            // produce defense: light manual cannon
            planet.GetDefenseSystem().ProduceDefenseLightManualCannon(out string info);

            // exit
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult Attack(int planetId, int userId, int planetIdTarget)
        {
            // get entities from database
            Models.Planet planet = PlanetProvider.GetPlanet(planetId);
            User user = UserProvider.GetUser(userId, true);

            // get fleet
            List<Ship> planetShips = user.Ship.Where(ship => ship.ShipPlanetId.Equals(planetId)).ToList();

            // add event
            string info = planet.Attack(planetShips, planetIdTarget);

            // show view
            return View("Home", new HomeViewModel(CurrentUser, info));
        }

        [HttpPost]
        public IActionResult Scan(int planetId, int userId, int planetIdTarget)
        {
            // get entities from database
            Models. Planet planet = PlanetProvider.GetPlanet(planetId);
            User user = UserProvider.GetUser(userId, true);

            // get fleet
            List<Ship> planetShips = user.Ship.Where(ship => ship.ShipPlanetId.Equals(planetId)).ToList();

            // add event
            string info = planet.Scan(planetShips, planetIdTarget);

            // show view
            return View("Home", new HomeViewModel(CurrentUser, info));
        }
    }
}
