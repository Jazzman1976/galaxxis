using Galaxxis.Core.Extensions.PlanetExtensions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Galaxxis.Web.Views.Shared.Components.ResourceBar
{
    public class ResourceBarViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Data.Models.Planet planet)
        {
            await Task.Run(() => { /* do asynchon things here */ });
            ResourceBarModel model = new() { Planet = planet };            
            return View("ResourceBar", model);
        }
    }
}
