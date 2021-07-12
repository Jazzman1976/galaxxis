using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Home
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            HomeViewModel model 
                = new HomeViewModel();

            return View("Home", model);
        }
    }
}
