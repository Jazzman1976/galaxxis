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
            int planetId = 0;
            if (int.TryParse(id, out planetId))
            {
                return View("PowerSupply", new PowerSupplyViewModel(planetId, _currentUserService));
            }
            else
            {
                return new EmptyResult();
            }
        }

        [HttpPost]
        public IActionResult BuildPowerSupply(int planetId)
        {
            PowerSupplyViewModel model = new PowerSupplyViewModel(planetId, _currentUserService);
            model.Planet.GetPowerSupply().Build(out string info);
            model.InfoMessage = info;
            return View("PowerSupply", model);
        }
    }
}
