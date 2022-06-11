using Galaxxis.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Galaxxis.Core.Provider
{
    public static class SystemProvider
    {
        /// <summary>
        /// Get a system with all planets and the moons of the planets
        /// </summary>
        /// <param name="id">The id of the system to get</param>
        /// <returns></returns>
        public static Data.Models.System GetSystem(int id)
        {
            Data.Models.System system = null;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                system = context.System

                    // get planets
                    .Include(system => system.Planet)

                    // get moons
                    .ThenInclude(planet => planet.Moon)

                    // return first system with data
                    .FirstOrDefault(system => system.SystemId.Equals(id));
            }
            return system;
        }
    }
}
