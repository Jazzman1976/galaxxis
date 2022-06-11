using Galaxxis.Web.Code.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Universe
{
    [Authorize]
    public class UniverseController : Controller
    {
        public IActionResult Index()
        {
            return View("Universe", new UniverseViewmodel());
        }

        [HttpPost]
        public RedirectResult GoGalaxy(int galaxy)
        {
            return new RedirectResult($"/galaxy/index/{galaxy}");
        }
    }
}
