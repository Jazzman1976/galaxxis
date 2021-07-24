using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Admin
{
    [Authorize]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            AdminViewModel model = new AdminViewModel();
            return View("Admin", model);
        }

        [HttpPost]
        public IActionResult Index(AdminViewModel model) 
        {
            if (model == null)
            {
                model = new AdminViewModel();
            }

            if (model.TickerStatus.Equals("running"))
            {
                Core.Engine.Ticker.Instance.Start();
            }
            else
            {
                Core.Engine.Ticker.Instance.Stop();
            }

            return View("Admin", model);
        }
    }
}
