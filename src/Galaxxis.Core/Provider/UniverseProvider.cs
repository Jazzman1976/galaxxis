using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Provider
{
    public static class UniverseProvider
    {
        public static Universe GetFirstUniverse()
        {
            Universe firstUniverse;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                firstUniverse = context.Universe.First();
            }
            return firstUniverse;
        }
    }
}
