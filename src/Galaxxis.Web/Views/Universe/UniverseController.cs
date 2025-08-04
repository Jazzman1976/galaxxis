using Galaxxis.Web.Code.Identity;
using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Universe
{
    [Authorize]
    public class UniverseController : Controller
    {
        private ICurrentUserService _currentUserService;

        public UniverseController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public IActionResult Index()
        {
            return View("Universe", new UniverseViewmodel(_currentUserService));
        }

        [HttpPost]
        public RedirectResult GoGalaxy(int galaxy)
        {
            return new RedirectResult($"/galaxy/index/{galaxy}");
        }
    }
}
