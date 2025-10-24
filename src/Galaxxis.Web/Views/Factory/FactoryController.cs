using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Factory
{
    [Authorize]
    public class FactoryController : Controller
    {
        private ICurrentUserService _currentUserService;
        public FactoryController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public ActionResult Index(string id) 
        {
            if (int.TryParse(id, out int planetId))
            {
                FactoryViewModel model = new(planetId, _currentUserService);
                return View("Factory", model);
            }
            else
            {
                return new EmptyResult();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuildFactory(int planetId)
        {
            FactoryViewModel model = new(planetId, _currentUserService);
            model.Planet.GetFactory(true).Build(out string info);
            TempData["InfoMessage"] = info ?? string.Empty;
            return RedirectToAction(nameof(Index), new { id = planetId });
        }
    }
}
