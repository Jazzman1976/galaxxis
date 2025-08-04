using Galaxxis.Core.Provider;
using Galaxxis.Web.Code.Services.CurrentUserService;
using System.Collections.Generic;

namespace Galaxxis.Web.Views.Universe
{
    public class UniverseViewmodel
    {
        public Data.Models.Universe Universe { get; }
        public List<Data.Models.Galaxy> PlayerGalaxies { get; } = new List<Data.Models.Galaxy>();

        public UniverseViewmodel(ICurrentUserService currentUserService)
        {
            this.Universe = UniverseProvider.GetFirstUniverse();
            Data.Models.User user = currentUserService.GetGalaxxisUserAsync().GetAwaiter().GetResult();
            foreach (Data.Models.Planet planet in user.Planet)
            {
                if (planet.PlanetSystem.SystemGalaxy.GalaxyUniverse.UniverseId != this.Universe.UniverseId)
                {
                    continue;
                }

                // add galaxy to player galaxies if not already added
                if (!this.PlayerGalaxies.Exists(g => g.GalaxyId == planet.PlanetSystem.SystemGalaxy.GalaxyId))
                {
                    this.PlayerGalaxies.Add(planet.PlanetSystem.SystemGalaxy);
                }
            }
        }
    }
}
