using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.OreMine
{
    [Authorize]
    public class OreMineController(ICurrentUserService currentUserService) : Controller
    {
        public ActionResult Index(string id) 
        {
            if (int.TryParse(id, out int planetId))
            {
                OreMineViewModel model = new(planetId, currentUserService);
                return View("OreMine", model);
            }
            else
            {
                return new EmptyResult();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuildOreMine(int planetId)
        {
            OreMineViewModel model = new(planetId, currentUserService);
            model.Planet.GetOreMine().Build(out string info);
            model.InfoMessage = info;
            return PartialView("_OreMineBuildPanel", model);
        }
    }
}
