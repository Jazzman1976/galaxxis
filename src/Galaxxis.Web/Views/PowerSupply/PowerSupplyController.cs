using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.PowerSupply
{
    [Authorize]
    public class PowerSupplyController : Controller
    {
        private ICurrentUserService _currentUserService;
        public PowerSupplyController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public ActionResult Index(string id) 
        {
            if (int.TryParse(id, out int planetId))
            {
                PowerSupplyViewModel model = new(planetId, _currentUserService);
                return View("PowerSupply", model);
            }
            else
            {
                return new EmptyResult();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuildPowerSupply(int planetId)
        {
            PowerSupplyViewModel model = new(planetId, _currentUserService);
            model.Planet.GetPowerSupply(true).Build(out string info);
            TempData["InfoMessage"] = info ?? string.Empty;
            TempData["Status"] = "Building";
            return RedirectToAction(nameof(Index), new { id = planetId });
        }
    }
}
