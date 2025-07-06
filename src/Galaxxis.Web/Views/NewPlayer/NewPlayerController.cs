using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.NewPlayer
{
    public class NewPlayerController : Controller
    {
        public IActionResult Index()
        {
            return View("NewPlayer");
        }
    }
}
