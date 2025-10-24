using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.CrystalMine
{
    [Authorize]
    public class CrystalMineController : Controller
    {
        private ICurrentUserService _currentUserService;
        public CrystalMineController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public ActionResult Index(string id) 
        {
            if (int.TryParse(id, out int planetId))
            {
                CrystalMineViewModel model = new(planetId, _currentUserService);
                return View("CrystalMine", model);
            }
            else
            {
                return new EmptyResult();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuildCrystalMine(int planetId)
        {
            CrystalMineViewModel model = new(planetId, _currentUserService);
            model.Planet.GetCrystalMine(true).Build(out string info);
            TempData["InfoMessage"] = info ?? string.Empty;
            return RedirectToAction(nameof(Index), new { id = planetId });
        }
    }
}
