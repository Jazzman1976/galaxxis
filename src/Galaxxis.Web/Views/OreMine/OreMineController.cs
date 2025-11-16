using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Web.Code.Services.CurrentUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.IO;
using System.Threading.Tasks;


namespace Galaxxis.Web.Views.OreMine
{
    [Authorize]
    public class OreMineController(
        ICurrentUserService _currentUserService, 
        ICompositeViewEngine _viewEngine) : Controller
    {
        public ActionResult Index(string id) 
        {
            if (int.TryParse(id, out int planetId))
            {
                OreMineViewModel model = new(planetId, _currentUserService);
                return View("OreMine", model);
            }
            else
            {
                return new EmptyResult();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuildOreMine(int planetId)
        {
            // init new model
            OreMineViewModel model = new(planetId, _currentUserService);

            // attempt to build ore mine
            model.Planet
                .GetOreMine(true) // needs to use the same planet instance as the model for partial view rendering
                .Build(out string info);
            model.InfoMessage = info;

            // return updated partial views
            string buildPanelHtml = await RenderPartialViewToString("_OreMineBuildPanel", model);
            string resourceBarHtml = await RenderPartialViewToString("_ResourceBar", model.Planet);
            return Json(new 
            { 
                BuildPanelHtml = buildPanelHtml, 
                ResourceBarHtml = resourceBarHtml
            });
        }

        // Helper method to render partial view to string
        private async Task<string> RenderPartialViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using var writer = new StringWriter();
            var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);

            if (viewResult.View == null)
                throw new ArgumentNullException($"{viewName} does not match any available view");

            var viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                ViewData,
                TempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return writer.GetStringBuilder().ToString();
        }
    }
}
