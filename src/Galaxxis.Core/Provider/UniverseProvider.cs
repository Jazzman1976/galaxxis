using Galaxxis.Data.Models;
using Microsoft.EntityFrameworkCore;
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
    }
}
