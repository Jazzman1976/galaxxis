using Galaxxis.Core.Exceptions;
using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Galaxxis.Core.Business.Provider
{
    public static class PlanetProvider
    {
        public static Planet GetPlanet(int id, bool updateRessources = false)
        {
            Planet planet = null;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                planet = context.Planet

                    // get moons
                    .Include(planet => planet.Moon)

                    // get system
                    .Include(planet => planet.PlanetSystem)

                    // get galaxy
                    .ThenInclude(system => system.SystemGalaxy)

                    // get universe
                    .ThenInclude(galaxy => galaxy.GalaxyUniverse)

                    // return first planet with data
                    .FirstOrDefault(planet => planet.PlanetId.Equals(id));
            }
            if (planet == null)
            {
                throw new PlanetNotFoundException();
            }

            if (updateRessources)
            {
                planet.UpdateRessources();
            }

            return planet;
        }
    }
}
