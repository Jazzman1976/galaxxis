using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            Models.Home.HomeViewModel model 
                = new Models.Home.HomeViewModel();

            return View("Home", model);
        }
    }
}
