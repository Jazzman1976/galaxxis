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
            int planetId = 0;
            if (int.TryParse(id, out planetId))
            {
                return View("OreMine", new OreMineViewModel(planetId, _currentUserService));
            }
            else
            {
                return new EmptyResult();
            }
        }

        [HttpPost]
        public IActionResult BuildOreMine(int planetId)
        {
            OreMineViewModel model = new OreMineViewModel(planetId, _currentUserService);
            model.Planet.GetOreMine().Build(out string info);
            model.InfoMessage = info;
            return View("OreMine", model);
        }
    }
}
