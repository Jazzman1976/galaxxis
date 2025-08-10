using Galaxxis.Core.Provider;
using Galaxxis.Web.Code.Services.CurrentUserService;
using System.Collections.Generic;

namespace Galaxxis.Web.Views.GalaxxisSystem
{
    public class GalaxxisSystemViewModel
    {
        public Data.Models.System System { get; }
        public List<Data.Models.Planet> UserPlanets { get; } = new List<Data.Models.Planet>();


        public GalaxxisSystemViewModel(int systemId, ICurrentUserService currentUserService)
        {
            this.System = SystemProvider.GetSystem(systemId);
            Data.Models.User user = currentUserService.GetGalaxxisUserAsync().GetAwaiter().GetResult();
            foreach (Data.Models.Planet planet in user.Planet)
            {
                if (planet.PlanetSystem.SystemId != this.System.SystemId)
                {
                    continue;
                }

                // add planet to user planets if not already added
                if (!this.UserPlanets.Exists(p => p.PlanetId == planet.PlanetId))
                {
                    this.UserPlanets.Add(planet);
                }
            }
        }
    }
}
