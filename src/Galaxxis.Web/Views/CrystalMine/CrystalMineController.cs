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
            int planetId = 0;
            if (int.TryParse(id, out planetId))
            {
                return View("CrystalMine", new CrystalMineViewModel(planetId, _currentUserService));
            }
            else
            {
                return new EmptyResult();
            }
        }

        [HttpPost]
        public IActionResult BuildCrystalMine(int planetId)
        {
            CrystalMineViewModel model = new CrystalMineViewModel(planetId, _currentUserService);
            model.Planet.GetCrystalMine().Build(out string info);
            model.InfoMessage = info;
            return View("CrystalMine", model);
        }
    }
}
