using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxxis.Core.Provider
{
    public static class GalaxyProvider
    {
        public static List<Galaxy> GetGalaxies(int universeId)
        {
            List<Galaxy> galaxies;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                galaxies = context.Galaxy
                    .Where(galaxy => galaxy.GalaxyUniverseId == universeId)
                    .ToList();
            }
            return galaxies;
        }
    }
}
