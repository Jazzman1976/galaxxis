using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.NeutronAccelerator
{
    [Authorize]
    public class NeutronAcceleratorController : Controller
    {
        private ICurrentUserService _currentUserService;
        public NeutronAcceleratorController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public ActionResult Index(string id) 
        {
            if (int.TryParse(id, out int planetId))
            {
                NeutronAcceleratorViewModel model = new(planetId, _currentUserService);
                return View("NeutronAccelerator", model);
            }
            else
            {
                return new EmptyResult();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuildNeutronAccelerator(int planetId)
        {
            NeutronAcceleratorViewModel model = new(planetId, _currentUserService);
            model.Planet.GetNeutronAccelerator(true).Build(out string info);
            TempData["InfoMessage"] = info ?? string.Empty;
            return RedirectToAction(nameof(Index), new { id = planetId });
        }
    }
}
