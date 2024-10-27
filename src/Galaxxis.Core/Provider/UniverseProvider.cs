using Galaxxis.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Galaxxis.Core.Provider
{
    public static class UniverseProvider
    {
        /// <summary>
        /// Get the first universe with all galaxies
        /// </summary>
        public static Universe GetFirstUniverse()
        {
            Universe firstUniverse;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                firstUniverse = context.Universe
                    .Include(universe => universe.Galaxy)
                    .First();
            }
            return firstUniverse;
        }

        public static Universe GetUniverseById(int id)
        {
            Universe theUniverse;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                theUniverse = context.Universe
                    .Include(universe => universe.Galaxy)
                    .First(u => u.UniverseId == id);
            }
            return theUniverse;
        }

        public static List<Universe> GetAllUniverses()
        { 
            List<Universe> universes = new();
            using (GalaxxisContext context = new GalaxxisContext())
            {
                universes = context.Universe.ToList();
            }
            return universes;
        }
    }
}
