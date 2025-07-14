using Galaxxis.Core.Provider;
using Galaxxis.Data.Models;
using Galaxxis.Web.Code.Identity;
using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Mvc;
using System;
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
            var model = new NewPlayerViewModel
            {
                Universes = UniverseProvider.GetAllUniverses()
            };
            return View("NewPlayer", model);
        }

        public async Task<IActionResult> Spawn(NewPlayerViewModel model)
        {
            Models.Planet randomPlanet = PlanetProvider
                .FindRandomNobodyPlanetInUniverse(model.UniverseId);

            AppUser currentUser = await _currentUserService.GetCurrentUserAsync();
            Models.User user = UserProvider.GetUser(currentUser.Email, includeData: true);

            model.HasAnyPlanet = PlanetProvider.AssignPlanetToUser(
                randomPlanet.PlanetId,
                user.UserId);

            return View("NewPlayer", model);
        }
    }
}
