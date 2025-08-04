using Galaxxis.Core.Provider;
using Galaxxis.Web.Code.Services.CurrentUserService;
using System.Collections.Generic;

namespace Galaxxis.Web.Views.Galaxy
{
    public class GalaxyViewmodel
    {
        public Data.Models.Galaxy Galaxy { get; }
        public List<Data.Models.System> UserSystems { get; } = new List<Data.Models.System>();

        public GalaxyViewmodel(int galaxyId, ICurrentUserService currentUserService)
        {
            this.Galaxy = GalaxyProvider.GetGalaxy(galaxyId);

            Data.Models.User user = currentUserService.GetGalaxxisUserAsync().GetAwaiter().GetResult();
            foreach (Data.Models.Planet planet in user.Planet)
            {
                if (planet.PlanetSystem.SystemGalaxy.GalaxyId != this.Galaxy.GalaxyId)
                {
                    continue;
                }

                // add system to user systems if not already added
                if (!this.UserSystems.Exists(s => s.SystemId == planet.PlanetSystem.SystemId))
                {
                    this.UserSystems.Add(planet.PlanetSystem);
                }
            }
        }
    }
}
