using Galaxxis.Core.Exceptions;
using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Galaxxis.Core.Provider
{
    public static class PlanetProvider
    {
        /// <summary>
        /// Get a planet instance
        /// </summary>
        /// <param name="updateRessources">default: false</param>
        public static Planet GetPlanet(
            int id, 
            bool updateRessources = false, 
            GalaxxisContext context = null)
        {
            // get planet from context
            Planet planet = null;
            if (context == null)
            {
                // create own context and repeat
                using (GalaxxisContext newContext = new GalaxxisContext())
                {
                    planet = GetPlanet(id, updateRessources, newContext);
                }
            }
            else
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

            // assure planet
            if (planet == null)
            {
                throw new PlanetNotFoundException();
            }

            // update ressources
            if (updateRessources)
            {
                planet.UpdateRessources();
            }

            // exit
            return planet;
        }
    }
}
