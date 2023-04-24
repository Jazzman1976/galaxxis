using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxxis.Web.Views.Start
{
    public class StartController : Controller
    {
        public IActionResult Index()
        {
            return View("Start");
        }
    }
}
