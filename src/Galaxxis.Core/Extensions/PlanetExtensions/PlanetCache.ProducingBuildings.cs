using Galaxxis.Core.Classes.Buildings.ProducingBuildings;
using Galaxxis.Data.Models;
using System.Collections.Generic;

namespace Galaxxis.Core.Extensions.PlanetExtensions
{
    public static partial class PlanetCache
    {
        public static void RemoveProducingBuildings(this Planet planet)
        {
            planet.RemoveShipyard();
        }

        #region Shipyard --------------------------------------------------------------------------

        private static readonly Dictionary<int, Shipyard> _planetShipyard
            = new Dictionary<int, Shipyard>();

        public static Shipyard GetShipyard(this Planet planet, bool forceReload = false)
        {
            if (!_planetShipyard.ContainsKey(planet.PlanetId) || forceReload)
            {
                _planetShipyard[planet.PlanetId] = new Shipyard(planet);
            }
            return _planetShipyard[planet.PlanetId];
        }

        public static void RemoveShipyard(this Planet planet)
        {
            _planetShipyard.Remove(planet.PlanetId);
        }

        #endregion
    }
}
