using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.OreMine
{
    [Authorize]
    public class OreMineController : Controller
    {
        private ICurrentUserService _currentUserService;
        public OreMineController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public ActionResult Index(string id) 
        {
            if (int.TryParse(id, out int planetId))
            {
                OreMineViewModel model = new(planetId, _currentUserService);
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
            OreMineViewModel model = new(planetId, _currentUserService);
            model.IsBuilding = model.Planet.GetOreMine(true).Build(out string info);            
            model.InfoMessage = info ?? string.Empty;            
            return PartialView("_OreMineBuildPanel", model);
        }
    }
}
