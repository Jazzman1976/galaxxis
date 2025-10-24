using Galaxxis.Core.Static;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Galaxxis.Web.Views.Shared.Components.ResourceBar
{
    public class ResourceBarViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Data.Models.Planet planet)
        {
            await Task.Run(() => { /* do asynchon things here */ });
            ResourceBarModel model = new() { Planet = planet };

            System.Diagnostics.Debug.WriteLine(
                $"{DateTime.Now.ToString(Constants.DEBUG_TIME_FORMAT)}: "
                + $"RessourceBar invoked on planet {planet.PlanetName}.");

            return View("ResourceBar", model);
        }
    }
}
