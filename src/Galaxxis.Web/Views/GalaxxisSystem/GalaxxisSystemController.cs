using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.GalaxxisSystem
{
    [Authorize]
    public class GalaxxisSystemController : Controller
    {
        private readonly ICurrentUserService _currentUserService;
        public GalaxxisSystemController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }
        public IActionResult Index(string id)
        {
            int systemId = 0;
            if (int.TryParse(id, out systemId))
            {
                return View("GalaxxisSystem", new GalaxxisSystemViewModel(systemId, _currentUserService));
            }
            else
            {
                return new EmptyResult();
            }
        }
        [HttpPost]
        public RedirectResult GoPlanet(int planet)
        {
            return new RedirectResult($"/planet/index/{planet}");
        }
    }
}
