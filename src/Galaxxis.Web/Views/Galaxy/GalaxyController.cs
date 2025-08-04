using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Galaxy
{
    [Authorize]
    public class GalaxyController : Controller
    {
        private ICurrentUserService _currentUserService;
        public GalaxyController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public IActionResult Index(string id)
        {
            int galaxyId = 0;
            if (int.TryParse(id, out galaxyId))
            {
                return View("Galaxy", new GalaxyViewmodel(galaxyId, _currentUserService));
            }
            else
            {
                return new EmptyResult();
            }
        }
    }
}
