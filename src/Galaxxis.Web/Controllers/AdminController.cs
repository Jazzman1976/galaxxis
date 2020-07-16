using Galaxxis.Web.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxxis.Web.Controllers
{
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
