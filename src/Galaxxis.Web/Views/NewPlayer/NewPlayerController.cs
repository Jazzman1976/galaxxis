using Galaxxis.Core.Provider;
using Galaxxis.Data.Models;
using Galaxxis.Web.Code.Identity;
using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Models = Galaxxis.Data.Models;

namespace Galaxxis.Web.Views.NewPlayer
{
    public class NewPlayerController : Controller
    {
        ICurrentUserService _currentUserService;
        public NewPlayerController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public IActionResult Index()
        {
            User user = _currentUserService.GetGalaxxisUserAsync().GetAwaiter().GetResult();
            var model = new NewPlayerViewModel
            {
                Universes = UniverseProvider.GetAllUniverses(),
                HasAnyPlanet = PlanetProvider.GetPlanetsOfUser(user.UserId).Any(),
            };
            return View("NewPlayer", model);
        }

        public IActionResult Spawn(NewPlayerViewModel model)
        {
            Models.Planet randomPlanet = PlanetProvider
                .FindRandomNobodyPlanetInUniverse(model.UniverseId);

            User user = _currentUserService.GetGalaxxisUserAsync().GetAwaiter().GetResult();

            model.HasAnyPlanet = PlanetProvider.AssignPlanetToUser(
                randomPlanet.PlanetId,
                user.UserId);

            return View("NewPlayer", model);
        }
    }
}
