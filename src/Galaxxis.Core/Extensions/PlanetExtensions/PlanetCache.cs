using Galaxxis.Data.Models;

namespace Galaxxis.Core.Extensions.PlanetExtensions
{
    public static partial class PlanetCache
    {
        public static void ClearCache(this Planet planet)
        {
            planet.RemoveRessourceBuildings();
            planet.RemoveProducingBuildings();            
        }
    }
}
