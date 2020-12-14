
namespace Galaxxis.Core.Business.Provider
{
    using Galaxxis.Core.Exceptions;
    using Galaxxis.Data.Models;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class PlanetProvider
    {
        public static Planet GetPlanet(int id)
        {
            Planet planet = Context.Instance.Database.Planet

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

            if (planet == null)
            {
                throw new PlanetNotFoundException();
            }

            return planet;
        }
    }
}
