using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Galaxy
{
    [Authorize]
    public class GalaxyController : Controller
    {
        public IActionResult Index(string id)
        {
            int galaxyId = 0;
            if (int.TryParse(id, out galaxyId))
            {
                return View("Galaxy", new GalaxyViewmodel(galaxyId));
            }
            else
            {
                return new EmptyResult();
            }
        }
    }
}
