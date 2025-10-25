using Galaxxis.Core.Extensions.PlanetExtensions;
namespace Galaxxis.Web.Views.OreMine
{
    public class OreMineViewModel
    {
        public Data.Models.Planet Planet { get; set; }
        public bool IsBuilding { get; set; } = false;
        public string InfoMessage { get; set; } = string.Empty;

        public OreMineViewModel(
            int planetId, 
            Code.Services.CurrentUserService.ICurrentUserService currentUserService)
        {
            Data.Models.User user = currentUserService
                .GetGalaxxisUserAsync()
                .GetAwaiter()
                .GetResult();
            
            foreach (Data.Models.Planet p in user.Planet)
            {
                if (p.PlanetId == planetId)
                {
                    this.Planet = p;
                    this.IsBuilding = !p.GetOreMine().CanBuild;
                    break;
                }
            }
        }
    }
}
